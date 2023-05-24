using System;
using CaptainOfCheats.Config;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface.Components;
using UnityEngine;

namespace CaptainOfCheats.Cheats.Weather
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class WeatherTab : Tab, ICheatProviderTab
    {
        private readonly WeatherCheatProvider _weatherCheatProvider;

        public WeatherTab(NewInstanceOf<WeatherCheatProvider> weatherCheatProvider) : base(nameof(WeatherTab), SyncFrequency.OncePerSec)
        {
            _weatherCheatProvider = weatherCheatProvider.Instance;
        }

        public string Name => "Weather";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.WorldMap_svg;

        protected override void BuildUi()
        {
            var buttonsContainer = Builder
                .NewStackContainer("Buttons container")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(25f)
                .SetInnerPadding(Offset.Top(20f) + Offset.Bottom(10f))
                .PutToTopOf(this, 680f);

            var buttonGroupContainer = Builder
                .NewStackContainer("Buttons container")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .SetInnerPadding(Offset.All(10f));

            buttonGroupContainer.AppendTo(buttonsContainer, buttonGroupContainer.GetDynamicHeight());

            foreach (var cheatItem in _weatherCheatProvider.Cheats)
                switch (cheatItem)
                {
                    case CheatToggleCommand toggleCommand:
                        break;
                    case CheatButtonCommand cheatCommand:
                    {
                        CreateCheatButton(cheatItem, cheatCommand, buttonGroupContainer);
                        break;
                    }
                }
        }

        private void CreateCheatButton(ICheatCommandBase cheatItem, CheatButtonCommand cheatButtonCommand, StackContainer buttonGroupContainer)
        {
            var btn = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted(cheatItem.Title))
                .AddToolTip(cheatItem.Tooltip)
                .OnClick(cheatButtonCommand.Action);
            btn.AppendTo(buttonGroupContainer, btn.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
        }
    }
}