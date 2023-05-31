using System;
using CaptainOfCheats.Constants;
using CaptainOfCheats.Extensions;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.GameLoop;
using Mafi.Core.Gfx;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Factory;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface;
using UnityEngine;

namespace CaptainOfCheats.Cheats.Tools
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces)]
    public class StorageGodModeController : Mafi.Unity.InputControl.Tools.BaseEntityCursorInputController<IStaticEntity>
    {
        private readonly EntitiesIconRenderer _iconRenderer;
        private readonly ToolbarController _toolbarController;
        private readonly IEntitiesManager _entitiesManager;
        private CursorStyle _cursorStyle;

        public StorageGodModeController(
            ProtosDb protosDb,
            UnlockedProtosDbForUi unlockedProtosDb,
            ShortcutsManager shortcutsManager,
            IUnityInputMgr inputManager,
            CursorPickingManager cursorPickingManager,
            CursorManager cursorManager,
            AreaSelectionToolFactory areaSelectionToolFactory,
            IEntitiesManager entitiesManager,
            NewInstanceOf<EntityHighlighter> highlighter,
            ToolbarController toolbarController,
            IGameLoopEvents gameLoopEvents,
            EntitiesIconRenderer iconRenderer
        ) : base(protosDb, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter,
            (Option<NewInstanceOf<TransportTrajectoryHighlighter>>)Option.None, null)
        {
            _toolbarController = toolbarController;
            _entitiesManager = entitiesManager;
            _iconRenderer = iconRenderer;

            gameLoopEvents.RegisterRendererInitState(this, InitState);
        }

        private void InitState()
        {
            foreach (var entity in _entitiesManager.Entities)
            {
                if (entity is Storage storage)
                {
                    SetGodModeIconOnStorage(storage);
                }
            }
        }

        public override void RegisterUi(UiBuilder builder)
        {
            _toolbarController
                .AddLeftMenuButton("Storage God Mode", this, "Assets/Unity/UserInterface/EntityIcons/Storage.svg", 70f, manager => KeyBindings.EMPTY)
                .AddTooltip(new LocStrFormatted("[Captain of Cheats] Enable god mode on storage buildings. " +
                                                "When a storage has god mod enabled, drag the green slider to the right to get infinite product from that storage. " +
                                                "Drag the red slider to the left to destroy that product in the storage (or any coming in via transport)."));
            
            _cursorStyle = new CursorStyle("StorageGodModeControllerStyle", "Assets/Unity/UserInterface/EntityIcons/Storage.svg", new Vector2(14f, 14f));
            InitializeUi(builder, _cursorStyle, builder.Audio.Assign, ColorRgba.White, ColorRgba.Green);
            base.RegisterUi(builder);
        }

        protected override bool OnFirstActivated(IStaticEntity hoveredEntity, Lyst<IStaticEntity> selectedEntities, Lyst<SubTransport> selectedPartialTransports)
        {
            return false;
        }

        protected override void OnEntitiesSelected(IIndexable<IStaticEntity> selectedEntities, IIndexable<SubTransport> selectedPartialTransports, bool isAreaSelection, bool isLeftMouse)
        {
            if (selectedEntities.Count == 0) return;

            foreach (var storage in selectedEntities.Select(s => (Storage)s))
            {
                storage.SetGodMode(!storage.IsGodModeEnabled());
                SetGodModeIconOnStorage(storage);
            }
        }

        private void SetGodModeIconOnStorage(Storage storage)
        {
            if (storage.IsGodModeEnabled())
            {
                _iconRenderer.AddIcon(new IconSpec(IconsPaths.ToolbarCaptainWheel, ColorRgba.Cyan), storage);
            }
            else
            {
                _iconRenderer.RemoveIcon(new IconSpec(IconsPaths.ToolbarCaptainWheel, ColorRgba.Cyan), storage);
            }
        }

        protected override bool Matches(IStaticEntity entity, bool isAreaSelection, bool isLeftClick)
        {
            if (entity.IsDestroyed)
                return false;
            if (entity is IStaticEntity staticEntity && !staticEntity.IsConstructed)
                return false;

            if (entity is Transport) return false;

            if (entity is Storage) return true;

            return false;
        }
    }
}