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
    public class CaptainOfCheatsController : BaseWindowController<CaptainOfCheatsWindowView>, IToolbarItemInputController
    {
        private readonly ToolbarController _toolbarController;

        public CaptainOfCheatsController(IUnityInputMgr inputManager, IGameLoopEvents gameLoop, CaptainOfCheatsWindowView captainOfCheatsWindowView, ToolbarController toolbarController)
            : base(inputManager, gameLoop, captainOfCheatsWindowView)
        {
            _toolbarController = toolbarController;
        }


        public override void RegisterUi(UiBuilder builder)
        {
            _toolbarController.AddMainMenuButton("Captain Of Cheats", this, IconsPaths.ToolbarCaptainWheel, 1337f, _ => KeyBindings.FromKey(KbCategory.Tools, KeyCode.F8));
            base.RegisterUi(builder);
        }

        public bool IsVisible => true;
        public bool DeactivateShortcutsIfNotVisible => false;
        public event Action<IToolbarItemInputController> VisibilityChanged;
    }
}