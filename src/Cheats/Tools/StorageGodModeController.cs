using CaptainOfCheats.Constants;
using CaptainOfCheats.Extensions;
using CaptainOfCheats.ReimplementedBaseClasses;
using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Gfx;
using Mafi.Core.Input;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface;
using UnityEngine;

namespace CaptainOfCheats.Cheats.Tools
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces)]
    public class StorageGodModeController : BaseEntityCursorInputController<IStaticEntity>
    {
        private readonly EntitiesIconRenderer _iconRenderer;
        private readonly ToolbarController _toolbarController;
        private CursorStyle _cursorStyle;

        public StorageGodModeController(ToolbarController toolbarController, ShortcutsManager shortcutsManager, IUnityInputMgr inputManager, CursorPickingManager cursorPickingManager,
            CursorManager cursorManager,
            AreaSelectionToolFactory areaSelectionToolFactory, IEntitiesManager entitiesManager, ObjectHighlighter highlighter, EntitiesRenderer entitiesRenderer, EntitiesIconRenderer iconRenderer)
            : base(shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, entitiesRenderer, false)
        {
            _toolbarController = toolbarController;
            _iconRenderer = iconRenderer;
        }

        public override void RegisterUi(UiBuilder builder)
        {
            _cursorStyle = new CursorStyle("StorageGodModeControllerStyle", "Assets/Unity/UserInterface/EntityIcons/Storage.svg", new Vector2(14f, 14f));
            InitializeUi(builder, _cursorStyle, builder.Audio.Assign, ColorRgba.White, ColorRgba.Green);

            _toolbarController
                .AddLeftMenuButton("Storage God Mode", this, "Assets/Unity/UserInterface/EntityIcons/Storage.svg", 70f, manager => KeyBindings.EMPTY)
                .AddTooltip(new LocStrFormatted("[Captain of Cheats] Enable god mode on storage buildings. " +
                                                "When a storage has god mod enabled, drag the green slider to the right to get infinite product from that storage. " +
                                                "Drag the red slider to the left to destroy that product in the storage (or any coming in via transport)."));
        }
        

        protected override void OnEntitiesSelected(ImmutableArray<IStaticEntity> immutableArray, bool isAreaSelection, bool isLeftClick)
        {
            if (immutableArray.Length == 0) return;

            foreach (var storage in immutableArray.Select(s => (Storage)s))
            {
                storage.SetGodMode(!storage.IsGodModeEnabled());

                if (storage.IsGodModeEnabled())
                    _iconRenderer.AddIcon(new IconSpec(IconsPaths.ToolbarCaptainWheel, ColorRgba.Cyan), storage);
                else
                    _iconRenderer.RemoveIcon(new IconSpec(IconsPaths.ToolbarCaptainWheel, ColorRgba.Cyan), storage);
            }
        }

        protected override Option<InputCommand> ScheduleCommand(IStaticEntity entity, bool isAreaSelection, bool isLeftClick)
        {
            return Option<InputCommand>.None;
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