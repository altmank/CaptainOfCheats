using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface.Components;
using UnityEngine;

namespace CaptainOfCheats.Cheats.Shipyard
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class ShipyardCheatTab : Tab, ICheatProviderTab
    {
        private readonly ShipyardCheatProvider _shipyardCheatProvider;
        private readonly IEnumerable<ProductProto> _productProtos;
        private readonly FleetCheatProvider _fleetCheatProvider;
        private readonly ProtosDb _protosDb;
        private float _quantity = 250;
        private ProductProto.ID? _selectedProduct;

        public ShipyardCheatTab(NewInstanceOf<ShipyardCheatProvider> productCheatProvider, NewInstanceOf<FleetCheatProvider> fleetCheatProvider, ProtosDb protosDb) : base(nameof(ShipyardCheatTab),
            SyncFrequency.OncePerSec)
        {
            _shipyardCheatProvider = productCheatProvider.Instance;
            _fleetCheatProvider = fleetCheatProvider.Instance;
            _protosDb = protosDb;
            _productProtos = _protosDb.Filter<ProductProto>(proto => proto.CanBeLoadedOnTruck).OrderBy(x => x);
        }

        public string Name => "Shipyard";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.CargoShip_svg;

        protected override void BuildUi()
        {
            var tabContainer = CreateStackContainer();
            
            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Shipyard Product Storage"), new LocStrFormatted("Add or remove products in the Shipyard storage."), Offset.Zero);
            var sectionTitlesContainer = Builder
                .NewStackContainer("shipyardContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .AppendTo(tabContainer, offset: Offset.All(0), size: 30);

            var quantitySectionTitle = Builder.CreateSectionTitle(new LocStrFormatted("Quantity"), new LocStrFormatted("Set the quantity of the product that will be affected by your add or remove product operation."));
            quantitySectionTitle.AppendTo(sectionTitlesContainer,  quantitySectionTitle.GetPreferedWidth(), Mafi.Unity.UiFramework.Offset.Left(10));
            
            var productSectionTitle = Builder.CreateSectionTitle(new LocStrFormatted("Product"), new LocStrFormatted("Select the product to add/remove from your shipyard."));
            productSectionTitle.AppendTo(sectionTitlesContainer, productSectionTitle.GetPreferedWidth(), Offset.Left(245));
            
            var quantityAndProductContainer = Builder
                .NewStackContainer("quantityAndProductContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .AppendTo(tabContainer, offset: Offset.Left(10), size: 30);
            
            var quantitySlider = BuildQuantitySlider();
            quantitySlider.AppendTo(quantityAndProductContainer, new Vector2(200, 28f), ContainerPosition.LeftOrTop);
            
            var buildProductSelector = BuildProductSelector();
            buildProductSelector.AppendTo(quantityAndProductContainer, new Vector2(200, 28f), ContainerPosition.LeftOrTop, Offset.Left(100));

            var thirdRowContainer = Builder
                .NewStackContainer("secondRowContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .AppendTo(tabContainer,offset: Offset.Left(10), size: 30);

            var spawnProductBtn = BuildAddProductBtn();
            spawnProductBtn.AppendTo(thirdRowContainer, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
            
            Panel horSep = this.Builder.NewPanel("separator").AppendTo<Panel>(tabContainer, new Vector2?(new Vector2(630f, 20f)), ContainerPosition.MiddleOrCenter, Offset.Top(20));
            this.Builder.NewIconContainer("left").SetIcon("Assets/Unity/UserInterface/General/HorizontalGradientToLeft48.png", false).PutToLeftMiddleOf<IconContainer>((IUiElement) horSep, new Vector2(300f, 1f));
            this.Builder.NewIconContainer("symbol").SetIcon("Assets/Unity/UserInterface/General/Tradable128.png").PutToCenterMiddleOf<IconContainer>((IUiElement) horSep, new Vector2(20f, 20f));
            this.Builder.NewIconContainer("right").SetIcon("Assets/Unity/UserInterface/General/HorizontalGradientToRight48.png", false).PutToRightMiddleOf<IconContainer>((IUiElement) horSep, new Vector2(300f, 1f));
            
            Builder.AddSectionTitle(tabContainer, new LocStrFormatted("Main Ship"));
            var mainShipPanel = Builder.NewPanel("mainShipPanel").SetBackground(Builder.Style.Panel.ItemOverlay);
            mainShipPanel.AppendTo(tabContainer, size: 50f, Offset.All(0));

            var mainShipBtnContainer = Builder
                .NewStackContainer("mainShipBtnContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .PutToLeftOf(mainShipPanel, 0.0f, Offset.Left(10f));
            
            var forceUnloadShipBtn = BuildForceUnloadShipyardShipButton();
            forceUnloadShipBtn.AppendTo(mainShipBtnContainer, forceUnloadShipBtn.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            
            var finishExplorationBtn = BuildFinishExplorationButton();
            finishExplorationBtn.AppendTo(mainShipBtnContainer, finishExplorationBtn.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
            
            var repairShipBtn = BuildRepairFleetButton();
            repairShipBtn.AppendTo(mainShipBtnContainer, repairShipBtn.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
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

        private Btn BuildAddProductBtn()
        {
            var btn = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Add Product"))
                .AddToolTip("Add the product at the quantity selected into your Shipyard storage.")
                .OnClick(() => _shipyardCheatProvider.AddItemToShipyard(_selectedProduct.Value, (int)_quantity));

            return btn;
            
        }

        private Btn BuildFinishExplorationButton()
        {
            var btn = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Finish Exploration"))
                .AddToolTip("Set your ship to do an action and then press this button and they will complete it immediately.")
                .OnClick(() => _fleetCheatProvider.FinishExploration());

            return btn;
        }

        private Btn BuildRepairFleetButton()
        {
            var btn = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Repair Ship"))
                .AddToolTip("Repair your main ship to full health.")
                .OnClick(() => _fleetCheatProvider.RepairFleet());

            return btn;
        }
        
        private Btn BuildForceUnloadShipyardShipButton()
        {
            var btn = Builder.NewBtnGeneral("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText("Force Unload Ship")
                .AddToolTip("Bypass shipyard cargo capacity check and forcefully unload your ship into your shipyard cargo.")
                .OnClick(() => _shipyardCheatProvider.ForceUnloadShipyardShip());
            return btn;
        }

        private Dropdwn BuildProductSelector()
        {
            var productDropdown = Builder
                .NewDropdown("ProductDropDown")
                .AddOptions(_productProtos.Select(x => x.Id.ToString().Replace("Product_", "")).ToList())
                .OnValueChange(i => _selectedProduct = _productProtos.ElementAt(i)?.Id);

            _selectedProduct = _productProtos.ElementAt(0)?.Id;

            return productDropdown;
        }

        private Slidder BuildQuantitySlider()
        {
            var sliderLabel = Builder
                .NewTxt("")
                .SetTextStyle(Builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleLeft);
            var qtySlider = Builder
                .NewSlider("qtySlider")
                .SimpleSlider(Builder.Style.Panel.Slider)
                .SetValuesRange(10f, 10000f)
                .OnValueChange(
                    qty => { sliderLabel.SetText(Math.Round(qty).ToString()); },
                    qty =>
                    {
                        sliderLabel.SetText(Math.Round(qty).ToString());
                        _quantity = qty;
                    })
                .SetValue(_quantity);


            sliderLabel.PutToRightOf(qtySlider, 90f, Offset.Right(-110f));

            return qtySlider;
        }
    }
}