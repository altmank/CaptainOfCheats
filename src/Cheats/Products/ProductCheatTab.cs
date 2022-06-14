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
using UnityEngine;

namespace CaptainOfCheats.Cheats.Products
{
    [GlobalDependency(RegistrationMode.AsEverything)]
    public class ProductCheatTab : Tab, ICheatProviderTab
    {
        private readonly ProductCheatProvider _cheatProvider;
        private readonly IEnumerable<ProductProto> _productProtos;
        private readonly ProtosDb _protosDb;
        private float _quantity = 250;
        private ProductProto.ID? _selectedProduct;

        public ProductCheatTab(NewInstanceOf<ProductCheatProvider> productCheatProvider, ProtosDb protosDb) : base(nameof(ProductCheatTab), SyncFrequency.OncePerSec)
        {
            _cheatProvider = productCheatProvider.Instance;
            _protosDb = protosDb;
            _productProtos = _protosDb.Filter<ProductProto>(proto => proto.CanBeLoadedOnTruck).OrderBy(x => x);
        }

        public string Name => "Products";
        public string IconPath => Assets.Unity.UserInterface.Toolbar.CargoShip_svg;

        protected override void BuildUi()
        {
            var topOf = CreateStackContainer();
            BuildQuantitySlider(topOf);
            BuildProductSelector(topOf);
            BuildSpawnButton(topOf);
            BuildRemoveButton(topOf);
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

        private void BuildSpawnButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Spawn Product in Shipyard"))
                .OnClick(() => _cheatProvider.AddItemToShipyard(_selectedProduct.Value, (int)_quantity));

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }

        private void BuildRemoveButton(StackContainer topOf)
        {
            var spawnProductBtn = Builder.NewBtn("button")
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("Remove Product in Shipyard"))
                .OnClick(() => _cheatProvider.RemoveItemFromShipYard(_selectedProduct.Value, (int)_quantity));

            spawnProductBtn.AppendTo(topOf, spawnProductBtn.GetOptimalSize(), ContainerPosition.LeftOrTop, Offset.Top(10f));
        }

        private void BuildProductSelector(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Product"), new LocStrFormatted("Select the product to spawn"));

            var productDropdown = Builder
                .NewDropdown("ProductDropDown")
                .AddOptions(_productProtos.Select(x => x.Id.ToString().Replace("Product_", "")).ToList())
                .OnValueChange(i => _selectedProduct = _productProtos.ElementAt(i)?.Id)
                .AppendTo(topOf, new Vector2(200, 28f), ContainerPosition.LeftOrTop);

            _selectedProduct = _productProtos.ElementAt(0)?.Id;
        }

        private void BuildQuantitySlider(StackContainer topOf)
        {
            Builder
                .AddSectionTitle(topOf, new LocStrFormatted("Quantity"), new LocStrFormatted("Drag slider to change item cheat amount quantity"));

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
                .SetValue(_quantity)
                .AppendTo(topOf, new Vector2(200, 28f), ContainerPosition.LeftOrTop);
            sliderLabel.PutToRightOf(qtySlider, 90f, Offset.Right(-110f));
        }
    }
}