using System;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Input;
using Mafi.Core.Terrain;
using Mafi.Unity;
using Mafi.Unity.Audio;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Factory;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface;
using UnityEngine;
using UnityEngine.EventSystems;
using Logger = CaptainOfCheats.Logging.Logger;

namespace CaptainOfCheats.ReimplementedBaseClasses
{
    //Reimplemented because it was marked as internal in the MAFI DLLs but is useful for our use.
    
    public abstract class BaseEntityCursorInputController<T> : 
    IToolbarItemInputController,
    IUnityInputController,
    IUnityUi
    where T : class, IAreaSelectableEntity, IRenderedEntity
  {
    public static readonly RelTile1i MAX_AREA_EDGE_SIZE;
    protected readonly ShortcutsManager ShortcutsManager;
    private readonly IUnityInputMgr m_inputManager;
    private readonly CursorPickingManager m_picker;
    private readonly CursorManager m_cursorManager;
    private readonly IEntitiesManager m_entitiesManager;
    private readonly EntityHighlighter m_highlighter;
    private readonly Option<TransportTrajectoryHighlighter> m_transportTrajectoryHighlighter;
    private readonly Lyst<AudioSource> m_sounds;
    private UiBuilder m_builder;
    private Option<Cursoor> m_cursor;
    private AudioSource m_invalidSound;
    private Option<T> m_toToggle;
    private Option<T> m_hoveredEntity;
    private readonly Lyst<InputCommand<bool>> m_pendingCmds;
    private readonly AreaSelectionTool m_areaSelectionTool;
    private readonly Lyst<T> m_selectedEntities;
    private readonly Lyst<SubTransport> m_selectedPartialTransports;
    private readonly Lyst<TransportTrajectory> m_partialTrajsTmp;
    private AudioInfo m_successSound;
    private ColorRgba m_colorHighlight;
    private ColorRgba m_colorConfirm;
    private ColorRgba? m_rightClickAreaColor;
    private bool m_isFirstUpdate;
    private bool m_enablePartialTransportsSelection;
    private bool m_isInstaActionDisabled;

    public event Action<IToolbarItemInputController> VisibilityChanged;

    public ControllerConfig Config => ControllerConfig.Tool;

    public bool IsVisible => true;

    public bool IsActive { get; private set; }

    protected BaseEntityCursorInputController(
      ShortcutsManager shortcutsManager,
      IUnityInputMgr inputManager,
      CursorPickingManager cursorPickingManager,
      CursorManager cursorManager,
      AreaSelectionToolFactory areaSelectionToolFactory,
      IEntitiesManager entitiesManager,
      NewInstanceOf<EntityHighlighter> highlighter,
      Option<NewInstanceOf<TransportTrajectoryHighlighter>> transportTrajectoryHighlighter)
    {
      this.m_sounds = new Lyst<AudioSource>();
      this.m_pendingCmds = new Lyst<InputCommand<bool>>();
      this.m_selectedEntities = new Lyst<T>();
      this.m_selectedPartialTransports = new Lyst<SubTransport>();
      this.m_partialTrajsTmp = new Lyst<TransportTrajectory>();
      this.ShortcutsManager = shortcutsManager;
      this.m_inputManager = inputManager;
      this.m_picker = cursorPickingManager;
      this.m_cursorManager = cursorManager;
      this.m_entitiesManager = entitiesManager;
      this.m_highlighter = highlighter.Instance;
      this.m_transportTrajectoryHighlighter = (Option<TransportTrajectoryHighlighter>) transportTrajectoryHighlighter.ValueOrNull?.Instance;
      this.m_areaSelectionTool = areaSelectionToolFactory.CreateInstance(new Action<RectangleTerrainArea2i, bool>(this.updateSelectionSync), new Action<RectangleTerrainArea2i, bool>(this.selectionDone), new Action(this.clearSelection), new Action(this.deactivateSelf));
      this.m_areaSelectionTool.SetEdgeSizeLimit(BaseEntityCursorInputController<T>.MAX_AREA_EDGE_SIZE);
    }

    protected void InitializeUi(
      UiBuilder builder,
      CursorStyle? cursorStyle,
      AudioInfo successSound,
      ColorRgba colorHighlight,
      ColorRgba colorConfirm)
    {
      this.m_builder = builder;
      this.m_successSound = successSound;
      this.m_colorHighlight = colorHighlight;
      this.m_colorConfirm = colorConfirm;
      if (cursorStyle.HasValue)
        this.m_cursor = (Option<Cursoor>) this.m_cursorManager.RegisterCursor(cursorStyle.Value);
      this.m_invalidSound = builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp);
      this.m_areaSelectionTool.SetLeftClickColor(colorHighlight);
    }

    protected void SetPartialTransportsSelection(bool isEnabled)
    {
      if (this.m_transportTrajectoryHighlighter.IsNone)
      {
        Log.Error("Transports trajectory highlighter must be set to allow partial transports.");
      }
      else
      {
        this.m_enablePartialTransportsSelection = isEnabled;
        this.m_areaSelectionTool.ForceSelectionChanged();
      }
    }

    protected void SetUpRightClickAreaSelection(ColorRgba color)
    {
      this.m_rightClickAreaColor = new ColorRgba?(color);
      this.m_areaSelectionTool.SetRightClickColor(color);
    }

    protected abstract bool Matches(T entity, bool isAreaSelection, bool isLeftClick);

    public abstract void RegisterUi(UiBuilder builder);

    protected virtual void OnHoverChanged(
      IIndexable<T> hoveredEntities,
      IIndexable<SubTransport> hoveredPartialTransports,
      bool isLeftClick)
    {
    }

    protected void SetInstaActionDisabled(bool isDisabled) => this.m_isInstaActionDisabled = isDisabled;

    public virtual void Activate()
    {
      if (this.IsActive)
        return;
      this.IsActive = true;
      this.m_cursor.ValueOrNull?.Show();
      this.m_areaSelectionTool.TerrainCursor.Activate();
      this.m_isFirstUpdate = !this.m_isInstaActionDisabled;
    }

    public virtual void Deactivate()
    {
      if (!this.IsActive)
        return;
      this.m_cursor.ValueOrNull?.Hide();
      this.m_picker.ClearPicked();
      this.m_pendingCmds.Clear();
      this.m_toToggle = (Option<T>) Option.None;
      this.m_areaSelectionTool.TerrainCursor.Deactivate();
      this.m_areaSelectionTool.Deactivate();
      this.clearSelection();
      this.m_isFirstUpdate = false;
      this.IsActive = false;
    }

    public virtual bool InputUpdate(IInputScheduler inputScheduler)
    {
      if (!this.IsActive)
      {
        Log.Error("Input update for non-active controller!");
        return false;
      }
      if (this.ShortcutsManager.IsSecondaryActionDown && !this.m_rightClickAreaColor.HasValue)
      {
        this.deactivateSelf();
        return true;
      }
      bool isFirstUpdate = this.m_isFirstUpdate;
      this.m_isFirstUpdate = false;
      if (this.m_pendingCmds.IsNotEmpty)
      {
        if (!this.handleCurrentCommand())
          return false;
        this.m_pendingCmds.Clear();
      }
      if (this.m_toToggle.IsNone)
      {
        if (this.m_areaSelectionTool.IsActive)
          return true;
        T obj = this.m_selectedEntities.Count == 1 ? this.m_selectedEntities.First : default (T);
        this.m_selectedEntities.Clear();
        this.m_selectedPartialTransports.Clear();
        this.m_hoveredEntity = this.m_picker.PickEntityAndSelect<T>(new CursorPickingManager.EntityPredicateReturningColor<T>(this.anyEntityMatcher));
        if (this.m_hoveredEntity.HasValue)
        {
          if (isFirstUpdate && !EventSystem.current.IsPointerOverGameObject())
          {
            this.m_selectedEntities.Clear();
            this.m_selectedPartialTransports.Clear();
            if (this.OnFirstActivated(this.m_hoveredEntity.Value, this.m_selectedEntities, this.m_selectedPartialTransports) && (this.m_selectedEntities.IsNotEmpty || this.m_selectedPartialTransports.IsNotEmpty))
            {
              this.OnEntitiesSelected((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, false, true);
              return true;
            }
          }
          this.m_selectedEntities.Add(this.m_hoveredEntity.Value);
          if (obj != this.m_hoveredEntity)
            this.OnHoverChanged((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, true);
          if (this.ShortcutsManager.IsPrimaryActionDown && !EventSystem.current.IsPointerOverGameObject())
          {
            this.m_toToggle = this.m_hoveredEntity;
            return true;
          }
        }
        else
        {
          if (this.ShortcutsManager.IsPrimaryActionDown)
          {
            this.m_areaSelectionTool.Activate(true);
            return true;
          }
          if (this.m_rightClickAreaColor.HasValue && this.ShortcutsManager.IsSecondaryActionDown)
          {
            this.m_areaSelectionTool.Activate(false);
            return true;
          }
          this.OnHoverChanged((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, true);
          return false;
        }
      }
      else
      {
        Option<T> option = this.m_picker.PickEntityAndSelect<T>(new CursorPickingManager.EntityPredicateReturningColor<T>(this.entityMatcher));
        if (this.ShortcutsManager.IsPrimaryActionUp)
        {
          this.m_toToggle = (Option<T>) Option.None;
          this.m_selectedEntities.Clear();
          this.m_selectedPartialTransports.Clear();
          if (option.IsNone || EventSystem.current.IsPointerOverGameObject())
          {
            this.OnHoverChanged((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, true);
            return true;
          }
          this.m_picker.ClearPicked();
          this.m_selectedEntities.Add(option.Value);
          if (this.m_selectedEntities.IsNotEmpty || this.m_selectedPartialTransports.IsNotEmpty)
            this.OnEntitiesSelected((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, false, true);
          return true;
        }
      }
      return false;
    }

    protected abstract bool OnFirstActivated(
      T hoveredEntity,
      Lyst<T> selectedEntities,
      Lyst<SubTransport> selectedPartialTransports);

    protected abstract void OnEntitiesSelected(
      IIndexable<T> selectedEntities,
      IIndexable<SubTransport> selectedPartialTransports,
      bool isAreaSelection,
      bool isLeftMouse);

    protected void RegisterPendingCommand(InputCommand cmd) => this.m_pendingCmds.Add((InputCommand<bool>) cmd);

    private void deactivateSelf() => this.m_inputManager.DeactivateController((IUnityInputController) this);

    private bool handleCurrentCommand()
    {
      bool flag = false;
      foreach (InputCommand<bool> pendingCmd in this.m_pendingCmds)
      {
        if (!pendingCmd.IsProcessedAndSynced)
          return false;
        flag |= pendingCmd.Result;
      }
      if (flag)
        this.playSuccessSound();
      else
        this.m_invalidSound.Play();
      return true;
    }

    private void playSuccessSound()
    {
      AudioSource audioSource = (AudioSource) null;
      foreach (AudioSource sound in this.m_sounds)
      {
        if (!sound.isPlaying)
          audioSource = sound;
      }
      if ((UnityEngine.Object) audioSource == (UnityEngine.Object) null)
      {
        audioSource = this.m_builder.AudioDb.GetClonedAudio(this.m_successSound);
        this.m_sounds.Add(audioSource);
      }
      audioSource.Play();
    }

    private bool anyEntityMatcher(T entity, out ColorRgba color)
    {
      if (!this.Matches(entity, false, true))
      {
        color = ColorRgba.Empty;
        return false;
      }
      color = this.m_colorHighlight;
      return true;
    }

    private bool entityMatcher(T entity, out ColorRgba color)
    {
      if (entity != this.m_toToggle || !this.Matches(entity, false, true))
      {
        color = ColorRgba.Empty;
        return false;
      }
      color = this.m_colorConfirm;
      return true;
    }

    private void selectionDone(RectangleTerrainArea2i area, bool isLeftClick)
    {
      if (this.m_selectedEntities.IsNotEmpty || this.m_selectedPartialTransports.IsNotEmpty)
        this.OnEntitiesSelected((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, true, isLeftClick);
      this.clearSelection();
      this.m_areaSelectionTool.Deactivate();
      this.OnHoverChanged((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, isLeftClick);
    }

    private void clearSelection()
    {
      this.m_selectedEntities.Clear();
      this.m_highlighter.ClearAllHighlights();
      this.m_transportTrajectoryHighlighter.ValueOrNull?.ClearAllHighlights();
      this.m_selectedPartialTransports.Clear();
    }

    private void updateSelectionSync(RectangleTerrainArea2i area, bool leftClick)
    {
      this.clearSelection();
      foreach (T entity in this.m_entitiesManager.GetAllEntitiesOfType<T>())
      {
        if (entity.IsSelected(area) && this.Matches(entity, true, leftClick) && !((object) entity is TransportPillar))
        {
          if (this.m_enablePartialTransportsSelection && entity is Mafi.Core.Factory.Transports.Transport originalTransport)
          {
            this.m_partialTrajsTmp.Clear();
            bool entireTrajectoryIsInArea;
            originalTransport.Trajectory.GetSubTrajectoriesInArea(area, this.m_partialTrajsTmp, out entireTrajectoryIsInArea);
            if (entireTrajectoryIsInArea)
            {
              addEntity(entity);
              Assert.That<Lyst<TransportTrajectory>>(this.m_partialTrajsTmp).IsEmpty<TransportTrajectory>();
            }
            else
            {
              foreach (TransportTrajectory transportTrajectory in this.m_partialTrajsTmp)
              {
                this.m_selectedPartialTransports.Add(new SubTransport(originalTransport, transportTrajectory));
                this.m_transportTrajectoryHighlighter.ValueOrNull?.HighlightTrajectory(transportTrajectory, this.m_colorHighlight);
              }
            }
          }
          else
            addEntity(entity);
        }
      }
      this.m_partialTrajsTmp.Clear();
      this.OnHoverChanged((IIndexable<T>) this.m_selectedEntities, (IIndexable<SubTransport>) this.m_selectedPartialTransports, leftClick);

      void addEntity(T entity)
      {
        this.m_selectedEntities.Add(entity);
        this.m_highlighter.Highlight((IRenderedEntity) entity, this.m_colorHighlight);
      }
    }

    static BaseEntityCursorInputController()
    {
      BaseEntityCursorInputController<T>.MAX_AREA_EDGE_SIZE = new RelTile1i(200);
    }
  }
  
}