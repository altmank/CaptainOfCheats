using System;
using System.Collections.Generic;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;

namespace CaptainOfCheats.Extensions
{
    public static class UIBuilderExtensions
    {
        public static Panel NewIncrementButtonGroup(this UiBuilder builder, Dictionary<int, Action<int>> incrementsAndActions)
        {
            var groupPanel = builder.NewPanel("IncrementPanel").SetBackground(builder.Style.Panel.ItemOverlay);

            var buttonGroupContainer = builder
                .NewStackContainer("Buttons container")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f);

            //Create negative increment buttons
            foreach (var incrementsAndAction in incrementsAndActions)
            {
                var newNegativeIncrementButton = builder.NewBtnGeneral("button")
                    .SetButtonStyle(builder.Style.Global.DangerBtn)
                    .SetText($"-{incrementsAndAction.Key}")
                    .OnClick(() => incrementsAndAction.Value(-incrementsAndAction.Key));
                newNegativeIncrementButton.AppendTo(buttonGroupContainer, newNegativeIncrementButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            }

            //Create Positive increment buttons
            foreach (var incrementsAndAction in incrementsAndActions)
            {
                var newPositiveIncrementButton = builder.NewBtnGeneral("button")
                    .SetButtonStyle(builder.Style.Global.PrimaryBtn)
                    .SetText($"+{incrementsAndAction.Key}")
                    .OnClick(() => incrementsAndAction.Value(incrementsAndAction.Key));
                newPositiveIncrementButton.AppendTo(buttonGroupContainer, newPositiveIncrementButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            }

            buttonGroupContainer.PutToLeftOf(groupPanel, 0.0f, Mafi.Unity.UiFramework.Offset.Left(10f));
            
            return groupPanel;
        }
        
    }
}