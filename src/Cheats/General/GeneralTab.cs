using System;
using System.Collections.Generic;
using CaptainOfCheats.Extensions;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface.Components;
using UnityEngine;

namespace CaptainOfCheats.Cheats.General
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class GeneralTab : Tab, ICheatProviderTab
    {
        private readonly UnityCheatProvider _unityCheatProvider;
        private readonly ResearchCheatProvider _researchCheatProvider;
        private readonly PopulationCheatProvider _populationCheatProvider;
        private readonly MaintenanceCheatProvider _maintenanceCheatProvider;
        private readonly InstantBuildCheatProvider _instantBuildCheatProvider;
        private readonly DiseaseCheatProvider _diseaseCheatProvider;
        private readonly Dict<SwitchBtn, Func<bool>> _switchBtns = new Dict<SwitchBtn, Func<bool>>();

        public GeneralTab(NewInstanceOf<InstantBuildCheatProvider> instantBuildCheatProvider,
            NewInstanceOf<MaintenanceCheatProvider> maintenanceCheatProvider,
            NewInstanceOf<PopulationCheatProvider> populationCheatProvider,
            NewInstanceOf<ResearchCheatProvider> researchCheatProvider,
            NewInstanceOf<UnityCheatProvider> unityCheatProvider,
            NewInstanceOf<DiseaseCheatProvider> diseaseCheatProvider
        ) : base(nameof(GeneralTab), SyncFrequency.OncePerSec)
        {
            _unityCheatProvider = unityCheatProvider.Instance;
            _researchCheatProvider = researchCheatProvider.Instance;
            _populationCheatProvider = populationCheatProvider.Instance;
            _maintenanceCheatProvider = maintenanceCheatProvider.Instance;
            _instantBuildCheatProvider = instantBuildCheatProvider.Instance;
            _diseaseCheatProvider = diseaseCheatProvider.Instance;
        }

        public string Name => "General";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.Settlement_svg;

        private Dictionary<int, Action<int>> PopulationIncrementButtonConfig =>
            new Dictionary<int, Action<int>>
            {
                { 5, _populationCheatProvider.ChangePopulation },
                { 25, _populationCheatProvider.ChangePopulation },
                { 50, _populationCheatProvider.ChangePopulation }
            };


        public override void RenderUpdate(GameTime gameTime)
        {
            RefreshValues();
            base.RenderUpdate(gameTime);
        }

        public override void SyncUpdate(GameTime gameTime)
        {
            RefreshValues();
            base.SyncUpdate(gameTime);
        }

        private void RefreshValues()
        {
            foreach (var kvp in _switchBtns) kvp.Key.SetIsOn(kvp.Value());
        }

        protected override void BuildUi()
        {
            var tabContainer = Builder
                .NewStackContainer("tabContainer")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetInnerPadding(Offset.All(15f))
                .SetItemSpacing(5f)
                .PutToTopOf(this, 0.0f);

            var firstRowContainer = Builder
                .NewStackContainer("firstRowContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .AppendTo(tabContainer, offset: Offset.All(0), size: 30);

            var instantModeToggle = NewToggleSwitch(
                "Instant Mode",
                "Set instant mode off (unchecked) or on (checked). Enables instant build, instant research, instant upgrades (shipyards, buildings, settlements, mines), instant vehicle construction, and instant repair when on.",
                toggleVal => _instantBuildCheatProvider.ToggleInstantMode(toggleVal),
                () => _instantBuildCheatProvider.IsInstantModeEnabled());
            instantModeToggle.AppendTo(firstRowContainer, new Vector2(instantModeToggle.GetWidth(), 25), ContainerPosition.LeftOrTop);

            var maintenanceToggle = NewToggleSwitch(
                "Maintenance",
                "Set Maintenance off (unchecked) or on (checked). If on, then your settlement will consume maintenance resources. If off, all consumption of maintenance will stop.",
                toggleVal => _maintenanceCheatProvider.ToggleMaintenance(toggleVal),
                () => _maintenanceCheatProvider.IsMaintenanceEnabled());
            maintenanceToggle.AppendTo(firstRowContainer, new Vector2(maintenanceToggle.GetWidth(), 25), ContainerPosition.LeftOrTop);
            maintenanceToggle.PutToRightOf(instantModeToggle, maintenanceToggle.GetWidth());
            
            var diseaseToggle = NewToggleSwitch(
                "Disease",
                "Set Disease off (unchecked) or on (checked). If off, every day if disease is detected it will be removed automatically. Toggle on/off is not persisted in your save game and resets every reload.",
                toggleVal => _diseaseCheatProvider.ToggleDisease(toggleVal),
                () => !_diseaseCheatProvider.IsDiseaseDisabled);
            diseaseToggle.AppendTo(firstRowContainer, new Vector2(diseaseToggle.GetWidth(), 25), ContainerPosition.LeftOrTop);
            diseaseToggle.PutToRightOf(instantModeToggle, diseaseToggle.GetWidth(), Offset.Right(-225));

            
            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Settlement Population"), new LocStrFormatted("Add or remove people from your population using the increment buttons."));
            var populationIncrementButtonGroup = Builder.NewIncrementButtonGroup(PopulationIncrementButtonConfig);
            populationIncrementButtonGroup.AppendTo(tabContainer, new float?(50f), Offset.All(0));


            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Research"));
            var researchPanel = Builder.NewPanel("researchPanel").SetBackground(Builder.Style.Panel.ItemOverlay);

            researchPanel.AppendTo(tabContainer, size: 50f, Offset.All(0));

            var thirdRowContainer = Builder
                .NewStackContainer("thirdRowContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .PutToLeftOf(researchPanel, 0.0f, Offset.Left(10f));
            
            var unlockCurrentResearchButton = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Finish Current Research"))
                .AddToolTip("Start research, and then use this command to instantly complete it. You can also use Instant Mode to complete started research immediately.")
                .OnClick(_researchCheatProvider.UnlockCurrentResearch);
            unlockCurrentResearchButton.AppendTo(thirdRowContainer, unlockCurrentResearchButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            var unlockAllResearchButton = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Unlock All Research"))
                .AddToolTip("Unlocks all research including research that requires discoveries to research.")
                .OnClick(_researchCheatProvider.UnlockAllResearch);
            unlockAllResearchButton.AppendTo(thirdRowContainer, unlockAllResearchButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            
            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Other"));
            var otherPanel = Builder.NewPanel("otherPanel").SetBackground(Builder.Style.Panel.ItemOverlay);
            otherPanel.AppendTo(tabContainer, size: 50f, Offset.All(0));
            
            var fourthRowContainer = Builder
                .NewStackContainer("fourthRowContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .PutToLeftOf(otherPanel, 0.0f, Offset.Left(10f));
            
            var addUnityButton = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Add 25 Unity"))
                .AddToolTip("Add 25 Unity to your current supply, it will not exceed your max Unity cap.")
                .OnClick(() => _unityCheatProvider.AddUnity(25));
            addUnityButton.AppendTo(fourthRowContainer, addUnityButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            
            RefreshValues();
        }

        private SwitchBtn NewToggleSwitch(string text, string tooltip, Action<bool> onToggleAction, Func<bool> isToggleEnabled)
        {
            var toggleBtn = Builder.NewSwitchBtn()
                .SetText(text)
                .AddTooltip(new LocStrFormatted(tooltip))
                .SetOnToggleAction(onToggleAction);

            _switchBtns.Add(toggleBtn, isToggleEnabled);

            return toggleBtn;
        }
    }
}