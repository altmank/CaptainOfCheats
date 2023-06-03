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
using Mafi.Unity.UserInterface.Components;
using UnityEngine;
using Assets = Mafi.Unity.Assets;

namespace CaptainOfCheats.Cheats.Terrain
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class TerrainCheatTab : Tab, ICheatProviderTab
    {
        private readonly ProtosDb _protosDb;
        private readonly TerrainCheatProvider _cheatProvider;
        private readonly Dict<SwitchBtn, Func<bool>> _switchBtns = new Dict<SwitchBtn, Func<bool>>();
        private readonly IOrderedEnumerable<LooseProductProto> _looseProductProtos;
        private bool _disableTerrainPhysicsOnMiningAndDumping = false;
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
            var tabContainer = CreateStackContainer();

            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Terrain"), new LocStrFormatted("Select the terrain to use when dumping."));
            var terrainSelector = BuildTerrainSelector(tabContainer);
            terrainSelector.AppendTo(tabContainer, new Vector2(150, 28f), ContainerPosition.LeftOrTop);
            var terrainPhysicsToggleSwitch = CreateTerrainPhysicsToggleSwitch();
            terrainPhysicsToggleSwitch.PutToRightOf(terrainSelector, terrainPhysicsToggleSwitch.GetWidth(), Offset.Right(-200f));
            var towerDesignationsToggleSwitch = CreateTerrainIgnoreMineTowerDesignationsToggleSwitch();
            towerDesignationsToggleSwitch.PutToRightOf(terrainPhysicsToggleSwitch, towerDesignationsToggleSwitch.GetWidth(), Offset.Right(-250f));


            var instantTerrainActions = Builder.NewPanel("instantTerrainActions").SetBackground(Builder.Style.Panel.ItemOverlay);
            instantTerrainActions.AppendTo(tabContainer, size: 50f, Offset.All(0));

            var instantTerrainButtonContainer = Builder
                .NewStackContainer("instantTerrainButtonContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .PutToLeftOf(instantTerrainActions, 0.0f, Offset.Left(10f));

            var buildMineButton = BuildMineButton();
            buildMineButton.AppendTo(instantTerrainButtonContainer, buildMineButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            var buildDumpButton = BuildDumpButton();
            buildDumpButton.AppendTo(instantTerrainButtonContainer, buildDumpButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            var buildChangeTerrainButton = BuildChangeTerrainButton();
            buildChangeTerrainButton.AppendTo(instantTerrainButtonContainer, buildChangeTerrainButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            

            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Other"));

            var otherTerrainActions = Builder.NewPanel("instantTerrainActions").SetBackground(Builder.Style.Panel.ItemOverlay);
            otherTerrainActions.AppendTo(tabContainer, size: 50f, Offset.All(0));
            var otherTerrainButtonContainer = Builder
                .NewStackContainer("otherTerrainButtonContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .PutToLeftOf(otherTerrainActions, 0.0f, Offset.Left(10f));

            var buildRefillGroundWaterButton = BuildRefillGroundWaterButton();
            buildRefillGroundWaterButton.AppendTo(otherTerrainButtonContainer, buildRefillGroundWaterButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            var buildRefillGroundCrudeButton = BuildRefillGroundCrudeButton();
            buildRefillGroundCrudeButton.AppendTo(otherTerrainButtonContainer, buildRefillGroundCrudeButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            var buildAddTreesButton = BuildAddsTreesButton();
            buildAddTreesButton.AppendTo(otherTerrainButtonContainer, buildAddTreesButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            
            var buildRemoveTreesButton = BuildRemoveTreesButton();
            buildRemoveTreesButton.AppendTo(otherTerrainButtonContainer, buildRemoveTreesButton.GetOptimalSize(), ContainerPosition.MiddleOrCenter);


        }

        private Dropdwn BuildTerrainSelector(StackContainer topOf)
        {
            var productDropdown = Builder
                .NewDropdown("TerrainDumpSelector")
                .AddOptions(_looseProductProtos.Select(x => x.Id.ToString().Replace("Product_", "")).ToList())
                .OnValueChange(i => _selectedLooseProductProto = (ProductProto.ID)_looseProductProtos.ElementAt(i)?.Id);

            _selectedLooseProductProto = _looseProductProtos.ElementAt(0)?.Id;

            return productDropdown;
        }
        
        private SwitchBtn CreateTerrainIgnoreMineTowerDesignationsToggleSwitch()
        {
            var toggleBtn = Builder.NewSwitchBtn()
                .SetText("Ignore Tower Designations")
                .AddTooltip("When instantly completing mining or dumping operations, ignore designations under mine tower control.")
                .SetOnToggleAction((toggleVal) => _ignoreMineTowerDesignations = toggleVal);

            _switchBtns.Add(toggleBtn, () => _ignoreMineTowerDesignations);

            return toggleBtn;
        }
        
        private SwitchBtn CreateTerrainPhysicsToggleSwitch()
        {
            var toggleBtn = Builder.NewSwitchBtn()
                .SetText("Disable Terrain Physics")
                .AddTooltip(
                    "When instantly completing mining or dumping designations, this toggle will indicate whether or not the game physics engine will affect the modified terrain. When turned on, expect very sharp edges on any terrain modifications you make. Note: Vehicles mining/dumping near no-physics terrain may cause no-physics terrain to start responding to physics.")
                .SetOnToggleAction((toggleVal) => _disableTerrainPhysicsOnMiningAndDumping = toggleVal);


            _switchBtns.Add(toggleBtn, () => _disableTerrainPhysicsOnMiningAndDumping);

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

        private Btn BuildMineButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Instant Mine"))
                .AddToolTip(
                    "All areas currently designated for mining will have their mining operation completed immediately. Results in no resources for the player.")
                .OnClick(() => _cheatProvider.CompleteAllMiningDesignations(_ignoreMineTowerDesignations, _disableTerrainPhysicsOnMiningAndDumping));

            return btn;
        }

        private Btn BuildDumpButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Instant Dump"))
                .AddToolTip(
                    "All areas currently designated for dumping will have their dump operation completed immediately. Requires no resources from the player.")
                .OnClick(() => _cheatProvider.CompleteAllDumpingDesignationsWithProduct((ProductProto.ID)_selectedLooseProductProto, _disableTerrainPhysicsOnMiningAndDumping, _ignoreMineTowerDesignations));

            return btn;
        }
        
        private Btn BuildChangeTerrainButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                    .SetButtonStyle(Style.Global.PrimaryBtn)
                    .SetText(new LocStrFormatted("Change Terrain"))
                    .AddToolTip(
                        "All areas currently designated for dumping will be used as markers for where to change the terrain selected in the terrain dropdown. The height of the terrain will not change, only the material of the top 1 layer. Useful for making dirt land for farms.")
                .OnClick(() => _cheatProvider.ChangeTerrain((ProductProto.ID)_selectedLooseProductProto, _ignoreMineTowerDesignations));
                

            return btn;
        }

        private Btn BuildRemoveTreesButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Style.Global.DangerBtn)
                .SetText("Remove Trees")
                .AddToolTip("Instantly remove all trees designated for removal by harvesters. Results in no resources for the player.")
                .OnClick(() => _cheatProvider.RemoveAllSelectedTrees());

            return btn;
        }
        
        private Btn BuildAddsTreesButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText("Add Trees")
                .AddToolTip("All areas currently designated for dumping and not under mining tower control " +
                            "will be used as markers for where to plant trees. " +
                            "Trees are planted with moderate spacing and aged 12 " +
                            "years (please check your local laws regarding minimum age of tree consent).")
                .OnClick(() => _cheatProvider.AddTreesToDumpingDesignations());

            return btn;
        }

        private Btn BuildRefillGroundWaterButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Fill Ground Water"))
                .AddToolTip("All ground reserves of water will be refilled to full capacity")
                .OnClick(() => _cheatProvider.RefillGroundWaterReserve());

            return btn;
        }

        private Btn BuildRefillGroundCrudeButton()
        {
            var btn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Fill Ground Crude"))
                .AddToolTip("All ground reserves of crude oil will be refilled to full capacity")
                .OnClick(() => _cheatProvider.RefillGroundCrudeReserve());

            return btn;
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
            foreach (var kvp in _switchBtns) kvp.Key.SetIsOn(kvp.Value());
        }
    }
}