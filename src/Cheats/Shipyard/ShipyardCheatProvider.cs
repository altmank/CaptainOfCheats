using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.World;

namespace CaptainOfCheats.Cheats.Shipyard
{
    public class ShipyardCheatProvider
    {
        private readonly IAssetTransactionManager _assetTransactionManager;
        private readonly ProtosDb _protosDb;
        private readonly TravelingFleetManager _travelingFleetManager;

        public ShipyardCheatProvider(IAssetTransactionManager assetTransactionManager, ProtosDb protosDb,  TravelingFleetManager travelingFleetManager)
        {
            _assetTransactionManager = assetTransactionManager;
            _protosDb = protosDb;
            _travelingFleetManager = travelingFleetManager;
        }

        public void AddItemToShipyard(ProductProto.ID product, int quantity = 1000)
        {
            var productProto = _protosDb.First<ProductProto>(p => p.Id == product);
            _assetTransactionManager.StoreProduct(new ProductQuantity(productProto.Value, new Quantity(quantity)), CreateReason.Cheated);
        }
        
        public void ForceUnloadShipyardShip()
        {
            var travelingFleet = _travelingFleetManager.TravelingFleet;
            var shipyard = travelingFleet.Dock;
            while (true)
            {
                ProductQuantity productQuantity = travelingFleet.TryUnloadCargo(Quantity.MaxValue,  new Set<ProductProto>());
                if (productQuantity.IsEmpty)
                {
                    break;
                }
                shipyard.StoreProduct(productQuantity);
            }
            

        }
    }
}