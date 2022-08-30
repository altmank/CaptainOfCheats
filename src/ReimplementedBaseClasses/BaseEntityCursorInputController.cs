using System;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using Mafi.Core.Terrain;
using Mafi.Unity;
using Mafi.Unity.Audio;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
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
        IUnityUi
        where T : class, IEntityWithPosition, IRenderedEntity
    {
        public static readonly RelTile1i MaxAreaEdgeSize;
        private readonly AreaSelectionTool _mAreaSelectionTool;
        private readonly CursorManager _mCursorManager;
        private readonly IEntitiesManager _mEntitiesManager;
        private readonly EntitiesRenderer _mEntitiesRenderer;
        private readonly Lyst<GameObject> _mHighlightedMbs;
        private readonly ObjectHighlighter _mHighlighter;
        private readonly IUnityInputMgr _mInputManager;
        private readonly Lyst<InputCommand<bool>> _mPendingCmds;
        private readonly CursorPickingManager _mPicker;
        private readonly Lyst<T> _mSelectedEntities;
        private readonly Lyst<AudioSource> _mSounds;
        private readonly bool _mUseShortcutOnHoveredActivation;
        protected readonly ShortcutsManager ShortcutsManager;
        private UiBuilder _mBuilder;
        private ColorRgba _mColorConfirm;
        private ColorRgba _mColorHighlight;
        private Option<Cursoor> _mCursor;
        private Option<T> _mHoveredEntity;
        private AudioSource _mInvalidSound;
        private bool _mIsFirstUpdate;
        private ColorRgba? _mRightClickAreaColor;
        private AudioInfo _mSuccessSound;
        private Option<T> _mToToggle;

        static BaseEntityCursorInputController()
        {
            MaxAreaEdgeSize = new RelTile1i(200);
        }

        public BaseEntityCursorInputController(
            ShortcutsManager shortcutsManager,
            IUnityInputMgr inputManager,
            CursorPickingManager cursorPickingManager,
            CursorManager cursorManager,
            AreaSelectionToolFactory areaSelectionToolFactory,
            IEntitiesManager entitiesManager,
            ObjectHighlighter highlighter,
            EntitiesRenderer entitiesRenderer,
            bool useShortcutOnHoveredActivation)
        {
            _mSounds = new Lyst<AudioSource>();
            _mPendingCmds = new Lyst<InputCommand<bool>>();
            _mHighlightedMbs = new Lyst<GameObject>();
            _mSelectedEntities = new Lyst<T>();
            ShortcutsManager = shortcutsManager;
            _mInputManager = inputManager;
            _mPicker = cursorPickingManager;
            _mCursorManager = cursorManager;
            _mEntitiesManager = entitiesManager;
            _mHighlighter = highlighter;
            _mEntitiesRenderer = entitiesRenderer;
            _mUseShortcutOnHoveredActivation = useShortcutOnHoveredActivation;
            _mAreaSelectionTool = areaSelectionToolFactory.CreateInstance(UpdateSelectionSync, SelectionDone, ClearSelection, DeactivateSelf);
            _mAreaSelectionTool.SetEdgeSizeLimit(MaxAreaEdgeSize);
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
            _mCursor.ValueOrNull?.Show();
            _mAreaSelectionTool.TerrainCursor.Activate();
            _mIsFirstUpdate = true;
        }

        public virtual void Deactivate()
        {
            if (!IsActive)
                return;
            _mCursor.ValueOrNull?.Hide();
            _mPicker.ClearPicked();
            _mPendingCmds.Clear();
            _mToToggle = (Option<T>)Option.None;
            _mAreaSelectionTool.TerrainCursor.Deactivate();
            _mAreaSelectionTool.Deactivate();
            ClearSelection();
            _mIsFirstUpdate = false;
            IsActive = false;
        }

        public virtual bool InputUpdate(IInputScheduler inputScheduler)
        {
            if (!IsActive)
            {
                Logger.Log.Error("Input update for non-active controller!");
                return false;
            }

            if (ShortcutsManager.IsSecondaryActionDown && !_mRightClickAreaColor.HasValue)
            {
                DeactivateSelf();
                return true;
            }

            var flag = _mIsFirstUpdate && _mUseShortcutOnHoveredActivation;
            _mIsFirstUpdate = false;
            if (_mPendingCmds.IsNotEmpty)
            {
                if (!HandleCurrentCommand())
                    return false;
                _mPendingCmds.Clear();
            }

            if (_mToToggle.IsNone)
            {
                if (_mAreaSelectionTool.IsActive)
                    return true;
                var obj = _mSelectedEntities.Count == 1 ? _mSelectedEntities.First : default;
                _mSelectedEntities.Clear();
                _mHoveredEntity = _mPicker.PickEntityAndSelect(new CursorPickingManager.EntityPredicateReturningColor<T>(AnyEntityMatcher));
                if (_mHoveredEntity.HasValue)
                {
                    _mSelectedEntities.Add(_mHoveredEntity.Value);
                    if (obj != _mHoveredEntity)
                        OnHoverChanged(_mSelectedEntities, true);
                    if (ShortcutsManager.IsPrimaryActionDown && !EventSystem.current.IsPointerOverGameObject())
                    {
                        _mToToggle = _mHoveredEntity;
                        return true;
                    }

                    if (flag && !EventSystem.current.IsPointerOverGameObject())
                    {
                        _mPicker.ClearPicked();
                        OnEntitiesSelected(ImmutableArray.Create(_mHoveredEntity.Value), false, true);
                        return true;
                    }
                }
                else
                {
                    if (ShortcutsManager.IsPrimaryActionDown)
                    {
                        _mAreaSelectionTool.Activate(true);
                        return true;
                    }

                    if (_mRightClickAreaColor.HasValue && ShortcutsManager.IsSecondaryActionDown)
                    {
                        _mAreaSelectionTool.Activate(false);
                        return true;
                    }

                    OnHoverChanged(_mSelectedEntities, true);
                    return false;
                }
            }
            else
            {
                var option = _mPicker.PickEntityAndSelect(new CursorPickingManager.EntityPredicateReturningColor<T>(EntityMatcher));
                if (ShortcutsManager.IsPrimaryActionUp)
                {
                    _mToToggle = (Option<T>)Option.None;
                    if (option.IsNone || EventSystem.current.IsPointerOverGameObject())
                    {
                        OnHoverChanged(_mSelectedEntities, true);
                        return true;
                    }

                    _mPicker.ClearPicked();
                    OnEntitiesSelected(ImmutableArray.Create(option.Value), false, true);
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
            _mBuilder = builder;
            _mSuccessSound = successSound;
            _mColorHighlight = colorHighlight;
            _mColorConfirm = colorConfirm;
            if (cursorStyle.HasValue)
                _mCursor = (Option<Cursoor>)_mCursorManager.RegisterCursor(cursorStyle.Value);
            _mInvalidSound = builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp);
            _mAreaSelectionTool.SetLeftClickColor(colorHighlight);
        }

        protected void SetUpRightClickAreaSelection(ColorRgba color)
        {
            _mRightClickAreaColor = color;
            _mAreaSelectionTool.SetRighClickColor(color);
        }

        protected abstract Option<InputCommand> ScheduleCommand(
            T entity,
            bool isAreaSelection,
            bool isLeftClick);

        protected abstract bool Matches(T entity, bool isAreaSelection, bool isLeftClick);

        protected virtual void OnHoverChanged(IIndexable<T> hoveredEntities, bool isLeftClick)
        {
        }

        protected virtual void OnEntitiesSelected(
            ImmutableArray<T> selectedEntities,
            bool isAreaSelection,
            bool isLeftClick)
        {
            foreach (var selectedEntity in _mSelectedEntities)
            {
                var option = ScheduleCommand(selectedEntity, isAreaSelection, isLeftClick);
                if (option.HasValue)
                    _mPendingCmds.Add(option.Value);
            }
        }

        protected void RegisterPendingCommand(InputCommand cmd)
        {
            _mPendingCmds.Add(cmd);
        }

        private void DeactivateSelf()
        {
            _mInputManager.DeactivateController(this);
        }

        private bool HandleCurrentCommand()
        {
            var flag = false;
            foreach (var pendingCmd in _mPendingCmds)
            {
                if (!pendingCmd.IsProcessedAndSynced)
                    return false;
                flag |= pendingCmd.Result;
            }

            if (flag)
                PlaySuccessSound();
            else
                _mInvalidSound.Play();
            return true;
        }

        private void PlaySuccessSound()
        {
            AudioSource audioSource = null;
            foreach (var sound in _mSounds)
                if (!sound.isPlaying)
                    audioSource = sound;
            if (audioSource == null)
            {
                audioSource = _mBuilder.AudioDb.GetClonedAudio(_mSuccessSound);
                _mSounds.Add(audioSource);
            }

            audioSource.Play();
        }

        private bool AnyEntityMatcher(T entity, out ColorRgba color)
        {
            if (!Matches(entity, false, true))
            {
                color = ColorRgba.Empty;
                return false;
            }

            color = _mColorHighlight;
            return true;
        }

        private bool EntityMatcher(T entity, out ColorRgba color)
        {
            if (entity != _mToToggle || !Matches(entity, false, true))
            {
                color = ColorRgba.Empty;
                return false;
            }

            color = _mColorConfirm;
            return true;
        }

        private void SelectionDone(RectangleTerrainArea2i area, bool isLeftClick)
        {
            OnEntitiesSelected(_mSelectedEntities.ToImmutableArray(), true, isLeftClick);
            ClearSelection();
            _mAreaSelectionTool.Deactivate();
            OnHoverChanged(_mSelectedEntities, isLeftClick);
        }

        private void ClearSelection()
        {
            _mHighlightedMbs.ForEachAndClear(x => _mHighlighter.RemoveHighlight(x, _mColorHighlight));
            _mSelectedEntities.Clear();
        }

        private void UpdateSelectionSync(RectangleTerrainArea2i area, bool leftClick)
        {
            ClearSelection();
            foreach (var entity in _mEntitiesManager.GetAllEntitiesOfType<T>())
            {
                EntityMb entityMb;
                if (area.Contains(entity.Position2f) && Matches(entity, true, leftClick) && _mEntitiesRenderer.TryGetMbFor(entity, out entityMb))
                {
                    _mSelectedEntities.Add(entity);
                    _mHighlightedMbs.Add(entityMb.gameObject);
                    _mHighlighter.Highlight(entityMb.gameObject, _mColorHighlight);
                }
            }

            OnHoverChanged(_mSelectedEntities, leftClick);
        }
    }
}