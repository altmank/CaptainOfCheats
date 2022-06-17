using CaptainOfCheats.Cheats.Generate;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;

namespace CaptainOfCheats.Cheats.InDevelopment
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class InDevelopmentTab : Tab
    {
        private readonly InDevelopmentProvider _inDevProvider;

        public InDevelopmentTab(NewInstanceOf<InDevelopmentProvider> inDevProvider) : base(nameof(InDevelopmentTab), SyncFrequency.OncePerSec)
        {
            _inDevProvider = inDevProvider.Instance;
        }

        public string Name => "In Dev";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.CSharp128_png;

        protected override void BuildUi()
        {
            var topOf = CreateStackContainer();

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
        


        

    }
}