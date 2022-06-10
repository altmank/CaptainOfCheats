using Mafi;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace CaptainOfCheats.Cheats.Products
{
    public class ProductCheatProvider
    {
        private readonly IAssetTransactionManager _assetTransactionManager;
        private readonly ProtosDb _protosDb;

        public ProductCheatProvider(IAssetTransactionManager assetTransactionManager, ProtosDb protosDb)
        {
            _assetTransactionManager = assetTransactionManager;
            _protosDb = protosDb;
        }

        public void AddItemToShipyard(ProductProto.ID product, int quantity = 1000)
        {
            var productProto = _protosDb.First<ProductProto>(p => p.Id == product);
            _assetTransactionManager.AddProduct(new ProductQuantity(productProto.Value, new Quantity(quantity)), CreateReason.Cheated);
        }

        public void RemoveItemFromShipYard(ProductProto.ID product, int quantity = 1000)
        {
            var productProto = _protosDb.First<ProductProto>(p => p.Id == product);
            _assetTransactionManager.RemoveProduct(new ProductQuantity(productProto.Value, new Quantity(quantity)), DestroyReason.Cheated);
        }
    }
}