using System;
using System.Collections.Generic;
using CaptainOfCheats.Extensions;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using UnityEngine;

namespace CaptainOfCheats.Cheats.Generate
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class GenerateCheatTab : Tab, ICheatProviderTab
    {
        private readonly ComputingCheatProvider _computingCheatProvider;
        private readonly ElectricityCheatProvider _electricityCheatProvider;
        private readonly UnityCheatProvider _unityCheatProvider;
        private int _computingTFlopGen;
        private int _unityGen;
        private int _kwGen;
        private const int SliderWidth = 585;

        public GenerateCheatTab(
            NewInstanceOf<ElectricityCheatProvider> electricityCheatProvider,
            NewInstanceOf<ComputingCheatProvider> computingCheatProvider,
            NewInstanceOf<UnityCheatProvider> unityCheatProvider
        ) : base(nameof(GenerateCheatTab), SyncFrequency.OncePerSec)
        {
            _unityCheatProvider = unityCheatProvider.Instance;
            _computingCheatProvider = computingCheatProvider.Instance;
            _electricityCheatProvider = electricityCheatProvider.Instance;
        }

        public string Name => "Generate";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.Power_svg;

        protected override void BuildUi()
        {
            var topOf = CreateStackContainer();
            BuildKwSlider(topOf);
            BuildTFlopSlider(topOf);
            BuildUnitySlider(topOf);
        }

        private StackContainer CreateStackContainer()
        {
            var topOf = Builder
                .NewStackContainer("container")
                .SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetSizeMode(StackContainer.SizeMode.Dynamic)
                .SetInnerPadding(Offset.All(15f))
                .SetItemSpacing(5f)
                .PutToTopOf(this, 0.0f);
            return topOf;
        }

        private void BuildKwSlider(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Perpetual KW Generation"), new LocStrFormatted("Use increment buttons to change perpetual KW amount generation"));

            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);
            sliderLabel.SetText(_kwGen.ToString());
            
            Action<int> zeroMinSetter = i =>
            {
                _kwGen = Math.Max(_kwGen + i, 0);
                _electricityCheatProvider.SetFreeElectricity(_kwGen);
                sliderLabel.SetText(_kwGen.ToString());
            };
            var incrementsAndActions = new Dictionary<int, Action<int>>()
            {
                {1, zeroMinSetter },
                {100, zeroMinSetter },
                {1000, zeroMinSetter },
                {100000, zeroMinSetter }
            };
            var newIncrementButtonGroup = Builder.NewIncrementButtonGroup(incrementsAndActions);
            newIncrementButtonGroup.AppendTo(topOf, new Vector2(SliderWidth, 50f), ContainerPosition.LeftOrTop);
            
            sliderLabel.PutToRightOf(newIncrementButtonGroup, 90f, Offset.Right(-100f));
        }

        private void BuildTFlopSlider(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Perpetual TFlop Generation"), new LocStrFormatted("Use increment buttons to change perpetual TFLOP amount generation"));

            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);
            sliderLabel.SetText(_computingTFlopGen.ToString());
            
            Action<int> zeroMinSetter = i =>
            {
                _computingTFlopGen = Math.Max(_computingTFlopGen + i, 0);
                _computingCheatProvider.SetFreeCompute(_computingTFlopGen);
                sliderLabel.SetText(_computingTFlopGen.ToString());
            };
            var incrementsAndActions = new Dictionary<int, Action<int>>()
            {
                {1, zeroMinSetter },
                {25, zeroMinSetter },
                {100, zeroMinSetter },
                {1000, zeroMinSetter }
            };
            var newIncrementButtonGroup = Builder.NewIncrementButtonGroup(incrementsAndActions);
            newIncrementButtonGroup.AppendTo(topOf, new Vector2(SliderWidth, 50f), ContainerPosition.LeftOrTop);
            
            
            sliderLabel.PutToRightOf(newIncrementButtonGroup, 90f, Offset.Right(-100f));
        }

        private void BuildUnitySlider(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Perpetual Unity Generation (per Month)"), new LocStrFormatted("Use increment buttons to change perpetual Unity amount generation"));

            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);

            sliderLabel.SetText(_unityGen.ToString());
            
            Action<int> zeroMinSetter = i =>
            {
                _unityGen = Math.Max(_unityGen + i, 0);
                _unityCheatProvider.SetFreeUPoints(_unityGen);
                sliderLabel.SetText(_unityGen.ToString());
            };
            var incrementsAndActions = new Dictionary<int, Action<int>>()
            {
                {1, zeroMinSetter },
                {5, zeroMinSetter },
                {10, zeroMinSetter },
                {25, zeroMinSetter },
                {100, zeroMinSetter }
            };
            var newIncrementButtonGroup = Builder.NewIncrementButtonGroup(incrementsAndActions);
            newIncrementButtonGroup.AppendTo(topOf, new Vector2(SliderWidth, 50f), ContainerPosition.LeftOrTop);
            
            sliderLabel.PutToRightOf(newIncrementButtonGroup, 90f, Offset.Right(-100f));
        }
    }
}