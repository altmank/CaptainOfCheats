using System;
using System.Linq;
using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using UnityEngine;
using Assets = Mafi.Unity.Assets;

namespace CaptainOfCheats.Cheats.Terrain
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class TerrainCheatTab : Tab, ICheatProviderTab
    {
        private readonly ProtosDb _protosDb;
        private readonly TerrainCheatProvider _cheatProvider;
        private bool _disableTerrainPhysicsOnMiningAndDumping = true;
        private readonly Dict<SwitchBtn, Func<bool>> _switchBtns = new Dict<SwitchBtn, Func<bool>>();
        private readonly IOrderedEnumerable<LooseProductProto> _looseProductProtos;
        private ProductProto.ID? _selectedLooseProductProto;
        private bool _ignoreMineTowerDesignations = true;

        public TerrainCheatTab(NewInstanceOf<TerrainCheatProvider> cheatProvider, ProtosDb _protosDb
        ) : base(nameof(TerrainCheatTab), SyncFrequency.OncePerSec)
        {
            this._protosDb = _protosDb;
            _cheatProvider = cheatProvider.Instance;
            
            _looseProductProtos = _protosDb.Filter<LooseProductProto>(proto => proto.CanBeLoadedOnTruck && proto.CanBeOnTerrain).OrderBy(x => x);
        }

        public string Name => "Terrain";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.Dumping_svg;

        protected override void BuildUi()
        {
            var topOf = CreateStackContainer();

            var terrainSelector = BuildTerrainSelector(topOf);
            var terrainPhysicsToggleSwitch = CreateTerrainPhysicsToggleSwitch();
            terrainPhysicsToggleSwitch.PutToRightOf(terrainSelector, terrainPhysicsToggleSwitch.GetWidth(),Offset.Right(-200f));
            var towerDesignationsToggleSwitch = CreateTerrainIgnoreMineTowerDesignationsToggleSwitch();
            towerDesignationsToggleSwitch.PutToRightOf(terrainPhysicsToggleSwitch, towerDesignationsToggleSwitch.GetWidth(),Offset.Right(-250f));

            BuildMineButton(topOf);
            BuildDumpButton(topOf);
            BuildRemoveTreesButton(topOf);
            BuildRefillGroundWaterButton(topOf);
            BuildRefillGroundCrudeButton(topOf);
            
            
        }

        private Dropdwn BuildTerrainSelector(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Terrain"), new LocStrFormatted("Select the terrain to use when dumping."));
            
            var productDropdown = Builder
                .NewDropdown("TerrainDumpSelector")
                .AddOptions(_looseProductProtos.Select(x => x.Id.ToString().Replace("Product_", "")).ToList())
                .OnValueChange(i => _selectedLooseProductProto = (ProductProto.ID)_looseProductProtos.ElementAt(i)?.Id)
                .AppendTo(topOf, new Vector2(150, 28f), ContainerPosition.LeftOrTop);

            _selectedLooseProductProto = _looseProductProtos.ElementAt(0)?.Id;

            return productDropdown;

        }
        
        private SwitchBtn CreateTerrainPhysicsToggleSwitch()
        {
            var toggleBtn = Builder.NewSwitchBtn()
                .SetText("Disable Terrain Physics")
                .AddTooltip("When instantly completing mining or dumping designations, this toggle will indicate whether or not the game physics engine will affect the modified terrain. When turned on, expect very sharp edges on any terrain modifications you make. Note: Vehicles mining/dumping near no-physics terrain may cause no-physics terrain to start responding to physics.")
                .SetOnToggleAction((toggleVal) => _disableTerrainPhysicsOnMiningAndDumping = toggleVal );

            
            _switchBtns.Add(toggleBtn, () => _disableTerrainPhysicsOnMiningAndDumping);

            return toggleBtn;
        }
        
        private SwitchBtn CreateTerrainIgnoreMineTowerDesignationsToggleSwitch()
        {
            var toggleBtn = Builder.NewSwitchBtn()
                .SetText("Ignore Tower Designations")
                .AddTooltip("When instantly completing mining or dumping operations, ignore designations under mine tower control.")
                .SetOnToggleAction((toggleVal) => _ignoreMineTowerDesignations = toggleVal );
            
            _switchBtns.Add(toggleBtn, () => _ignoreMineTowerDesignations);

            return toggleBtn;
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
        
        private void BuildMineButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Instantly Complete Mining Designations"))
                .AddToolTip("All areas currently designated for mining will have their dump operation completed immediately. Results in no resources for the player. WARNING: If terrain physics is turned on, be aware that large mining operations can take awhile to finish due to physics catching up.")
                .OnClick(() => _cheatProvider.CompleteAllMiningDesignations(_disableTerrainPhysicsOnMiningAndDumping, _ignoreMineTowerDesignations));

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }

        private void BuildDumpButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Instantly Complete Dumping Designations"))
                .AddToolTip("All areas currently designated for dumping will have their dump operation completed immediately. Requires no resources from the player. If terrain physics is turned on, the shape you create will be altered by terrain physics after the material spawns in.")
                .OnClick(() => _cheatProvider.CompleteAllDumpingDesignations((ProductProto.ID)_selectedLooseProductProto, _disableTerrainPhysicsOnMiningAndDumping, _ignoreMineTowerDesignations));

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }
        
        private void BuildRemoveTreesButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Remove All Selected Trees"))
                .OnClick(() => _cheatProvider.RemoveAllSelectedTrees());

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }
        private void BuildRefillGroundWaterButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Refill Ground Water Reserves"))
                .OnClick(() => _cheatProvider.RefillGroundWaterReserve());

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }
        
        private void BuildRefillGroundCrudeButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Refill Ground Crude Reserves"))
                .OnClick(() => _cheatProvider.RefillGroundCrudeReserve());

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }
        
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
            foreach (var kvp in _switchBtns) kvp.Key.SetState(kvp.Value());
        }

        
        
    }
}