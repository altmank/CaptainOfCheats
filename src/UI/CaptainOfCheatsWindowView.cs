using CaptainOfCheats.Cheats;
using CaptainOfCheats.Logging;
using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Localization;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface;
using UnityEngine;
using Logger = CaptainOfCheats.Logging.Logger;

namespace CaptainOfCheats.UI
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class CaptainOfCheatsWindowView : WindowView
    {
        private readonly ImmutableArray<ICheatProviderTab> _cheatTabs;
        private TabsContainer _tabsContainer;

        public CaptainOfCheatsWindowView(AllImplementationsOf<ICheatProviderTab> cheatTabs) : base("CaptainOfCheatsWindowView")
        {
            _cheatTabs = cheatTabs.Implementations.OrderBy(x => x.Name).ToImmutableArray();
            Logger.Log.Info($"Found {_cheatTabs.Length} cheat tabs");
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            //Apparently this shit makes tab changing work?
            _tabsContainer.SyncUpdate(gameTime);
            base.RenderUpdate(gameTime);
        }

        public override void SyncUpdate(GameTime gameTime)
        {
            //Apparently this shit makes tab changing work?
            _tabsContainer.RenderUpdate(gameTime);
            base.SyncUpdate(gameTime);
        }

        protected override void BuildWindowContent()
        {
            Logger.Log.Info("Started building cheat menu");
            SetTitle(new LocStrFormatted("Captain of Cheats Menu"));
            var size = new Vector2(680f, 400f);
            SetContentSize(size);
            PositionSelfToCenter();
            MakeMovable();

            _tabsContainer = Builder.NewTabsContainer(size.x.RoundToInt(), size.y.RoundToInt());
            foreach (var tab in _cheatTabs)
            {
                Logger.Log.Info($"Adding {tab.Name} tab to cheat menu");
                _tabsContainer.AddTab(tab.Name, new IconStyle(tab.IconPath, Builder.Style.Global.DefaultPanelTextColor), (Tab)tab);
            }

            _tabsContainer.PutTo(GetContentPanel());

            Logger.Log.Info("Finished building cheat menu");
        }
    }
}