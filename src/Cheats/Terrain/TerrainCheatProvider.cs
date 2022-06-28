using System.Linq;
using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;
using Mafi.Core.Terrain.Designation;
using Mafi.Core.Terrain.Trees;

namespace CaptainOfCheats.Cheats.Terrain
{
    public class TerrainCheatProvider
    {
        private readonly ProtosDb _protosDb;
        private readonly ITerrainDesignationsManager _terrainDesignationsManager;
        private readonly ITerrainDumpingManager _terrainDumpingManager;
        private readonly ITerrainMiningManager _terrainMiningManager;
        private readonly TreeManager _treeManager;
        private readonly VirtualResourceManager _virtualResourceManager;

        public TerrainCheatProvider(ITerrainDesignationsManager terrainDesignationsManager, ProtosDb protosDb, 
            ITerrainDumpingManager terrainDumpingManager, ITerrainMiningManager terrainMiningManager, TreeManager treeManager, VirtualResourceManager virtualResourceManager)
        {
            _terrainDesignationsManager = terrainDesignationsManager;
            _protosDb = protosDb;
            _terrainDumpingManager = terrainDumpingManager;
            _terrainMiningManager = terrainMiningManager;
            _treeManager = treeManager;
            _virtualResourceManager = virtualResourceManager;
        }

        public void AddTree()
        {
            
        }

        public void RefillGroundWaterReserve()
        {
            var groundWater = _protosDb.First<VirtualResourceProductProto>(x => x.Id == IdsCore.Products.Groundwater);

            var allGroundWaterResources = _virtualResourceManager.GetAllResourcesFor(groundWater.Value);

            foreach (var groundWaterResource in allGroundWaterResources)
            {
                groundWaterResource.AddAsMuchAs(groundWaterResource.Capacity);
                
            }
        }
        
        public void RefillGroundCrudeReserve()
        {
            var groundCrude = _protosDb.First<VirtualResourceProductProto>(x => x.Id == IdsCore.Products.VirtualCrudeOil);

            var allGroundCrudeResources = _virtualResourceManager.GetAllResourcesFor(groundCrude.Value);

            foreach (var resource in allGroundCrudeResources)
            {
                resource.AddAsMuchAs(resource.Capacity);
            }
        }

        public void RemoveAllSelectedTrees()
        {
            var treeIds = _treeManager.EnumerateSelectedTrees().ToList();

            for (var index = 0; index < treeIds.Count; index++)
            {
                var treeId = treeIds[index];
                _treeManager.TryRemoveTree(treeId);
            }
        }

        public void HarvestTreesInTerrainDesignation(TerrainDesignation designation)
        {
            var designationArea = designation.Area;

            var treesInArea = _treeManager.EnumerateTreesInArea(designationArea);

            var treeIds = treesInArea.ToList();

            for (var index = 0; index < treeIds.Count; index++)
            {
                var treeId = treeIds[index];
                _treeManager.TryRemoveTree(treeId);
            }
        }

        public void CompleteAllMiningDesignations(bool doNoTerrainPhysics = false, bool ignoreMineTowerDesignations = true)
        {
            var miningTerrainDesignations = _terrainMiningManager.MiningDesignations
                .Where(x => x.IsNotFulfilled);

            foreach (var designation in miningTerrainDesignations)
            {
                if (designation.ManagedByTowers.Count > 0 && ignoreMineTowerDesignations)
                {
                    continue;
                }
                HarvestTreesInTerrainDesignation(designation);
                SetHeightToMatchDesignation(designation, doNoTerrainPhysics);
            }
        }

        public void CompleteAllDumpingDesignations(ProductProto.ID looseMaterialProductId, bool doNoTerrainPhysics = false, bool ignoreMineTowerDesignations = true)
        {
            var looseProductProto = _protosDb.First<LooseProductProto>(x => x.Id == looseMaterialProductId);

            var dumpingDesignations = _terrainDumpingManager.DumpingDesignations
                .Where(x => x.IsNotFulfilled);

            foreach (var designation in dumpingDesignations)
            {
                if (designation.ManagedByTowers.Count > 0 && ignoreMineTowerDesignations)
                {
                    continue;
                }
                HarvestTreesInTerrainDesignation(designation);
                ChangeMaterial(designation, looseProductProto.Value);
                SetHeightToMatchDesignation(designation, doNoTerrainPhysics);
            }
        }
        
        public void ChangeTerrain(ProductProto.ID looseMaterialProductId, bool doNoTerrainPhysics = false, bool ignoreMineTowerDesignations = true)
        {
            var looseProductProto = _protosDb.First<LooseProductProto>(x => x.Id == looseMaterialProductId);

            var dumpingDesignations = _terrainDumpingManager.DumpingDesignations;

            foreach (var designation in dumpingDesignations)
            {
                if (designation.ManagedByTowers.Count > 0 && ignoreMineTowerDesignations)
                {
                    continue;
                }
                ChangeMaterial(designation, looseProductProto.Value);
            }
        }

        public void ChangeMaterial(TerrainDesignation terrainDesignation, LooseProductProto newTerrainMaterial)
        {
            var looseProductQuantity = new LooseProductQuantity(newTerrainMaterial, Quantity.MaxValue);
            var terrainThickness = looseProductQuantity.ToTerrainThickness();

            terrainDesignation.ForEachTile((tile, f) => { DumpTile(tile, ref terrainThickness, ThicknessTilesF.MaxValue, terrainDesignation, true); });
        }

        public void SetHeightToMatchDesignation(TerrainDesignation terrainDesignation, bool doNoTerrainPhysics)
        {
            terrainDesignation.ForEachTile((tile, f) => { tile.SetHeight(GetTargetHeight(terrainDesignation, tile.TileCoord), doNoTerrainPhysics: doNoTerrainPhysics); });
        }

        private HeightTilesF GetTargetHeight(TerrainDesignation terrainDesignation, Tile2i position)
        {
            if (terrainDesignation.ContainsPosition(position)) return terrainDesignation.GetTargetHeightAt(position);

            var dumpingDesignationAt = _terrainDumpingManager.GetDumpingDesignationAt(position);
            return dumpingDesignationAt.HasValue ? dumpingDesignationAt.Value.GetTargetHeightAt(position) : HeightTilesF.MinValue;
        }

        private void DumpTile(TerrainTile tile, ref TerrainMaterialThickness product, ThicknessTilesF maxDumped, TerrainDesignation terrainDesignation, bool doNotChangeHeight = false,
            bool doNotRaiseEvents = false,
            bool doNotDisruptTerrain = false)
        {
            var targetHeight = GetTargetHeight(terrainDesignation, tile.TileCoord);
            if (tile.Height >= targetHeight) return;

            var thicknessTilesF = targetHeight - tile.Height;
            thicknessTilesF = thicknessTilesF.Min(maxDumped);
            var thickness = thicknessTilesF.Min(product.Thickness);
            tile.DepositMaterialOnTop(product.Material, thickness, doNotRaiseEvents: doNotRaiseEvents, doNotDisruptTerrain: doNotDisruptTerrain, doNotChangeHeight: doNotChangeHeight);
            product = new TerrainMaterialThickness(product.Material, product.Thickness - thickness);
        }
    }
}