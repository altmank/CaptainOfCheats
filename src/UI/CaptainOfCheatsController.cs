using System;
using CaptainOfCheats.Constants;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using UnityEngine;

namespace CaptainOfCheats.UI
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class CaptainOfCheatsController : BaseWindowController<CaptainOfCheatsWindowView>, IToolbarItemController
    {
        private readonly ToolbarController _toolbarController;

        public CaptainOfCheatsController(IUnityInputMgr inputManager, IGameLoopEvents gameLoop,UiBuilder uiBuilder, CaptainOfCheatsWindowView captainOfCheatsWindowView, ToolbarController toolbarController)
            : base(inputManager, gameLoop, uiBuilder, captainOfCheatsWindowView)
        {
            _toolbarController = toolbarController;
        }


        public bool IsVisible => true;
        public bool DeactivateShortcutsIfNotVisible => false;

        event Action<IToolbarItemController> IToolbarItemController.VisibilityChanged
        {
            add
            {
            }

            remove
            {
            }
        }

        public void RegisterIntoToolbar(ToolbarController controller)
        {
            _toolbarController.AddMainMenuButton("Captain Of Cheats", this, IconsPaths.ToolbarCaptainWheel, 1337f, _ => KeyBindings.FromKey(KbCategory.Tools, ShortcutMode.Game, KeyCode.F8));
        }
    }
}