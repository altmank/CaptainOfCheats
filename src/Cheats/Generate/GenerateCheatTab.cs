using System;
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
        private float _computingTFlopGen;
        private float _kwGen;
        private readonly int _sliderWidth = 585;
        private float _unityGen;

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
                .AddSectionTitle(topOf, new LocStrFormatted("Perpetual KW Generation"), new LocStrFormatted("Drag slider to change perpetual KW amount generation"));

            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);
            var kwSlider = Builder
                .NewSlider("kwSlider")
                .SimpleSlider(Builder.Style.Panel.Slider)
                .SetValuesRange(0, 100000)
                .OnValueChange(
                    qty => { sliderLabel.SetText(Math.Round(qty).ToString()); },
                    qty =>
                    {
                        sliderLabel.SetText(Math.Round(qty).ToString());
                        _kwGen = qty;
                        _electricityCheatProvider.SetFreeElectricity((int)qty);
                    })
                .SetValue(_kwGen)
                .AppendTo(topOf, new Vector2(_sliderWidth, 28f), ContainerPosition.LeftOrTop);
            sliderLabel.PutToRightOf(kwSlider, 90f, Offset.Right(-110f));
        }

        private void BuildTFlopSlider(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Perpetual TFlop Generation"), new LocStrFormatted("Drag slider to change perpetual TFLOP amount generation"));

            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);
            var tflopSlider = Builder
                .NewSlider("tFlopSlider")
                .SimpleSlider(Builder.Style.Panel.Slider)
                .SetValuesRange(0, 1000)
                .OnValueChange(
                    qty => { sliderLabel.SetText(Math.Round(qty).ToString()); },
                    qty =>
                    {
                        sliderLabel.SetText(Math.Round(qty).ToString());
                        _computingTFlopGen = qty;
                        _computingCheatProvider.SetFreeCompute((int)qty);
                    })
                .SetValue(_computingTFlopGen)
                .AppendTo(topOf, new Vector2(_sliderWidth, 28f), ContainerPosition.LeftOrTop);
            sliderLabel.PutToRightOf(tflopSlider, 90f, Offset.Right(-110f));
        }

        private void BuildUnitySlider(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Perpetual Unity Generation (Not yet implemented)"), new LocStrFormatted("Drag slider to change perpetual Unity amount generation"));

            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);
            var unitySlider = Builder
                .NewSlider("unitySlider")
                .SimpleSlider(Builder.Style.Panel.Slider)
                .SetValuesRange(0, 1000)
                .OnValueChange(
                    qty => { sliderLabel.SetText(Math.Round(qty).ToString()); },
                    qty =>
                    {
                        sliderLabel.SetText(Math.Round(qty).ToString());
                        _unityGen = qty;
                        _unityCheatProvider.SetFreeUPoints((int)qty);
                    })
                .SetValue(_unityGen)
                .AppendTo(topOf, new Vector2(_sliderWidth, 28f), ContainerPosition.LeftOrTop);
            sliderLabel.PutToRightOf(unitySlider, 90f, Offset.Right(-110f));
        }
    }
}