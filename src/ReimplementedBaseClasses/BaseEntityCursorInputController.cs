using System;
using Mafi;
using Mafi.Collections;
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
        private readonly AreaSelectionTool m_areaSelectionTool;
        private readonly CursorManager m_cursorManager;
        private readonly IEntitiesManager m_entitiesManager;
        private readonly EntityHighlighter m_highlighter;
        private readonly IUnityInputMgr m_inputManager;
        private readonly Lyst<TransportTrajectory> m_partialTrajsTmp;
        private readonly Lyst<InputCommand<bool>> m_pendingCmds;
        private readonly CursorPickingManager m_picker;
        private readonly Lyst<T> m_selectedEntities;
        private readonly Lyst<SubTransport> m_selectedPartialTransports;
        private readonly Lyst<AudioSource> m_sounds;
        private readonly Option<TransportTrajectoryHighlighter> m_transportTrajectoryHighlighter;
        protected readonly ShortcutsManager ShortcutsManager;
        private UiBuilder m_builder;
        private ColorRgba m_colorConfirm;
        private ColorRgba m_colorHighlight;
        private Option<Cursoor> m_cursor;
        private bool m_enablePartialTransportsSelection;
        private Option<T> m_hoveredEntity;
        private AudioSource m_invalidSound;
        private bool m_isFirstUpdate;
        private bool m_isInstaActionDisabled;
        private ColorRgba? m_rightClickAreaColor;
        private AudioInfo m_successSound;
        private Option<T> m_toToggle;

        static BaseEntityCursorInputController()
        {
            MAX_AREA_EDGE_SIZE = new RelTile1i(200);
        }

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
            m_sounds = new Lyst<AudioSource>();
            m_pendingCmds = new Lyst<InputCommand<bool>>();
            m_selectedEntities = new Lyst<T>();
            m_selectedPartialTransports = new Lyst<SubTransport>();
            m_partialTrajsTmp = new Lyst<TransportTrajectory>();
            ShortcutsManager = shortcutsManager;
            m_inputManager = inputManager;
            m_picker = cursorPickingManager;
            m_cursorManager = cursorManager;
            m_entitiesManager = entitiesManager;
            m_highlighter = highlighter.Instance;
            m_transportTrajectoryHighlighter = (Option<TransportTrajectoryHighlighter>)transportTrajectoryHighlighter.ValueOrNull?.Instance;
            m_areaSelectionTool = areaSelectionToolFactory.CreateInstance(updateSelectionSync, selectionDone, clearSelection, deactivateSelf);
            m_areaSelectionTool.SetEdgeSizeLimit(MAX_AREA_EDGE_SIZE);
        }

        public bool IsActive { get; private set; }

        public event Action<IToolbarItemInputController> VisibilityChanged;

        public ControllerConfig Config => ControllerConfig.Tool;

        public bool IsVisible => true;

        public virtual void Activate()
        {
            if (IsActive)
                return;
            IsActive = true;
            m_cursor.ValueOrNull?.Show();
            m_areaSelectionTool.TerrainCursor.Activate();
            m_isFirstUpdate = !m_isInstaActionDisabled;
        }

        public virtual void Deactivate()
        {
            if (!IsActive)
                return;
            m_cursor.ValueOrNull?.Hide();
            m_picker.ClearPicked();
            m_pendingCmds.Clear();
            m_toToggle = (Option<T>)Option.None;
            m_areaSelectionTool.TerrainCursor.Deactivate();
            m_areaSelectionTool.Deactivate();
            clearSelection();
            m_isFirstUpdate = false;
            IsActive = false;
        }

        public virtual bool InputUpdate(IInputScheduler inputScheduler)
        {
            if (!IsActive)
            {
                Log.Error("Input update for non-active controller!");
                return false;
            }

            if (ShortcutsManager.IsSecondaryActionDown && !m_rightClickAreaColor.HasValue)
            {
                deactivateSelf();
                return true;
            }

            var isFirstUpdate = m_isFirstUpdate;
            m_isFirstUpdate = false;
            if (m_pendingCmds.IsNotEmpty)
            {
                if (!handleCurrentCommand())
                    return false;
                m_pendingCmds.Clear();
            }

            if (m_toToggle.IsNone)
            {
                if (m_areaSelectionTool.IsActive)
                    return true;
                var obj = m_selectedEntities.Count == 1 ? m_selectedEntities.First : default;
                m_selectedEntities.Clear();
                m_selectedPartialTransports.Clear();
                m_hoveredEntity = m_picker.PickEntityAndSelect(new CursorPickingManager.EntityPredicateReturningColor<T>(anyEntityMatcher));
                if (m_hoveredEntity.HasValue)
                {
                    if (isFirstUpdate && !EventSystem.current.IsPointerOverGameObject())
                    {
                        m_selectedEntities.Clear();
                        m_selectedPartialTransports.Clear();
                        if (OnFirstActivated(m_hoveredEntity.Value, m_selectedEntities, m_selectedPartialTransports) && (m_selectedEntities.IsNotEmpty || m_selectedPartialTransports.IsNotEmpty))
                        {
                            OnEntitiesSelected(m_selectedEntities, m_selectedPartialTransports, false, true);
                            return true;
                        }
                    }

                    m_selectedEntities.Add(m_hoveredEntity.Value);
                    if (obj != m_hoveredEntity)
                        OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, true);
                    if (ShortcutsManager.IsPrimaryActionDown && !EventSystem.current.IsPointerOverGameObject())
                    {
                        m_toToggle = m_hoveredEntity;
                        return true;
                    }
                }
                else
                {
                    if (ShortcutsManager.IsPrimaryActionDown)
                    {
                        m_areaSelectionTool.Activate(true);
                        return true;
                    }

                    if (m_rightClickAreaColor.HasValue && ShortcutsManager.IsSecondaryActionDown)
                    {
                        m_areaSelectionTool.Activate(false);
                        return true;
                    }

                    OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, true);
                    return false;
                }
            }
            else
            {
                var option = m_picker.PickEntityAndSelect(new CursorPickingManager.EntityPredicateReturningColor<T>(entityMatcher));
                if (ShortcutsManager.IsPrimaryActionUp)
                {
                    m_toToggle = (Option<T>)Option.None;
                    m_selectedEntities.Clear();
                    m_selectedPartialTransports.Clear();
                    if (option.IsNone || EventSystem.current.IsPointerOverGameObject())
                    {
                        OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, true);
                        return true;
                    }

                    m_picker.ClearPicked();
                    m_selectedEntities.Add(option.Value);
                    if (m_selectedEntities.IsNotEmpty || m_selectedPartialTransports.IsNotEmpty)
                        OnEntitiesSelected(m_selectedEntities, m_selectedPartialTransports, false, true);
                    return true;
                }
            }

            return false;
        }

        public abstract void RegisterUi(UiBuilder builder);

        protected void InitializeUi(
            UiBuilder builder,
            CursorStyle? cursorStyle,
            AudioInfo successSound,
            ColorRgba colorHighlight,
            ColorRgba colorConfirm)
        {
            m_builder = builder;
            m_successSound = successSound;
            m_colorHighlight = colorHighlight;
            m_colorConfirm = colorConfirm;
            if (cursorStyle.HasValue)
                m_cursor = (Option<Cursoor>)m_cursorManager.RegisterCursor(cursorStyle.Value);
            m_invalidSound = builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp);
            m_areaSelectionTool.SetLeftClickColor(colorHighlight);
        }

        protected void SetPartialTransportsSelection(bool isEnabled)
        {
            if (m_transportTrajectoryHighlighter.IsNone)
            {
                Log.Error("Transports trajectory highlighter must be set to allow partial transports.");
            }
            else
            {
                m_enablePartialTransportsSelection = isEnabled;
                m_areaSelectionTool.ForceSelectionChanged();
            }
        }

        protected void SetUpRightClickAreaSelection(ColorRgba color)
        {
            m_rightClickAreaColor = color;
            m_areaSelectionTool.SetRighClickColor(color);
        }

        protected abstract bool Matches(T entity, bool isAreaSelection, bool isLeftClick);

        protected virtual void OnHoverChanged(
            IIndexable<T> hoveredEntities,
            IIndexable<SubTransport> hoveredPartialTransports,
            bool isLeftClick)
        {
        }

        protected void SetInstaActionDisabled(bool isDisabled)
        {
            m_isInstaActionDisabled = isDisabled;
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

        protected void RegisterPendingCommand(InputCommand cmd)
        {
            m_pendingCmds.Add(cmd);
        }

        private void deactivateSelf()
        {
            m_inputManager.DeactivateController(this);
        }

        private bool handleCurrentCommand()
        {
            var flag = false;
            foreach (var pendingCmd in m_pendingCmds)
            {
                if (!pendingCmd.IsProcessedAndSynced)
                    return false;
                flag |= pendingCmd.Result;
            }

            if (flag)
                playSuccessSound();
            else
                m_invalidSound.Play();
            return true;
        }

        private void playSuccessSound()
        {
            AudioSource audioSource = null;
            foreach (var sound in m_sounds)
                if (!sound.isPlaying)
                    audioSource = sound;
            if (audioSource == null)
            {
                audioSource = m_builder.AudioDb.GetClonedAudio(m_successSound);
                m_sounds.Add(audioSource);
            }

            audioSource.Play();
        }

        private bool anyEntityMatcher(T entity, out ColorRgba color)
        {
            if (!Matches(entity, false, true))
            {
                color = ColorRgba.Empty;
                return false;
            }

            color = m_colorHighlight;
            return true;
        }

        private bool entityMatcher(T entity, out ColorRgba color)
        {
            if (entity != m_toToggle || !Matches(entity, false, true))
            {
                color = ColorRgba.Empty;
                return false;
            }

            color = m_colorConfirm;
            return true;
        }

        private void selectionDone(RectangleTerrainArea2i area, bool isLeftClick)
        {
            if (m_selectedEntities.IsNotEmpty || m_selectedPartialTransports.IsNotEmpty)
                OnEntitiesSelected(m_selectedEntities, m_selectedPartialTransports, true, isLeftClick);
            clearSelection();
            m_areaSelectionTool.Deactivate();
            OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, isLeftClick);
        }

        private void clearSelection()
        {
            m_selectedEntities.Clear();
            m_highlighter.ClearAllHighlights();
            m_transportTrajectoryHighlighter.ValueOrNull?.ClearAllHighlights();
            m_selectedPartialTransports.Clear();
        }

        private void updateSelectionSync(RectangleTerrainArea2i area, bool leftClick)
        {
            clearSelection();
            foreach (var entity in m_entitiesManager.GetAllEntitiesOfType<T>())
                if (entity.IsSelected(area) && Matches(entity, true, leftClick) && !((object)entity is TransportPillar))
                {
                    if (m_enablePartialTransportsSelection && entity is Transport originalTransport)
                    {
                        m_partialTrajsTmp.Clear();
                        bool entireTrajectoryIsInArea;
                        originalTransport.Trajectory.GetSubTrajectoriesInArea(area, m_partialTrajsTmp, out entireTrajectoryIsInArea);
                        if (entireTrajectoryIsInArea)
                        {
                            addEntity(entity);
                            Assert.That(m_partialTrajsTmp).IsEmpty();
                        }
                        else
                        {
                            foreach (var transportTrajectory in m_partialTrajsTmp)
                            {
                                m_selectedPartialTransports.Add(new SubTransport(originalTransport, transportTrajectory));
                                m_transportTrajectoryHighlighter.ValueOrNull?.HighlightTrajectory(transportTrajectory, m_colorHighlight);
                            }
                        }
                    }
                    else
                    {
                        addEntity(entity);
                    }
                }

            m_partialTrajsTmp.Clear();
            OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, leftClick);

            void addEntity(T entity)
            {
                m_selectedEntities.Add(entity);
                m_highlighter.Highlight(entity, m_colorHighlight);
            }
        }
    }
}