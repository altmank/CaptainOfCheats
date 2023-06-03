using System.Linq;
using System.Net;
using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Simulation;
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
        private readonly TerrainManager _terrainManager;
        private readonly ICalendar _calendar;
        private readonly TerrainOccupancyManager _terrainOccupancyManager;
        private readonly ITerrainMiningManager _terrainMiningManager;
        private readonly ITreesManager _treeManager;
        private readonly VirtualResourceManager _virtualResourceManager;

        public TerrainCheatProvider(ITerrainDesignationsManager terrainDesignationsManager, ProtosDb protosDb,
            ITerrainDumpingManager terrainDumpingManager, ITerrainMiningManager terrainMiningManager, ITreesManager treeManager, VirtualResourceManager virtualResourceManager,
            TerrainManager terrainManager, ICalendar calendar, TerrainOccupancyManager terrainOccupancyManager)
        {
            _terrainDesignationsManager = terrainDesignationsManager;
            _protosDb = protosDb;
            _terrainDumpingManager = terrainDumpingManager;
            _terrainMiningManager = terrainMiningManager;
            _treeManager = treeManager;
            _virtualResourceManager = virtualResourceManager;
            _terrainManager = terrainManager;
            _calendar = calendar;
            _terrainOccupancyManager = terrainOccupancyManager;
        }

        public void RefillGroundWaterReserve()
        {
            var groundWater = _protosDb.First<VirtualResourceProductProto>(x => x.Id == IdsCore.Products.Groundwater);

            var allGroundWaterResources = _virtualResourceManager.GetAllResourcesFor(groundWater.Value);

            foreach (var groundWaterResource in allGroundWaterResources) groundWaterResource.AddAsMuchAs(groundWaterResource.Capacity);
        }

        public void RefillGroundCrudeReserve()
        {
            var groundCrude = _protosDb.First<VirtualResourceProductProto>(x => x.Id == IdsCore.Products.VirtualCrudeOil);

            var allGroundCrudeResources = _virtualResourceManager.GetAllResourcesFor(groundCrude.Value);

            foreach (var resource in allGroundCrudeResources) resource.AddAsMuchAs(resource.Capacity);
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

        public void AddTreesToDumpingDesignations()
        {
            var dumpingDesignations = _terrainDumpingManager.DumpingDesignations;

            foreach (var designation in dumpingDesignations)
            {
                PlantTreesInTerrainDesignation(designation);
            }
        }

        private void HarvestTreesInTerrainDesignation(TerrainDesignation designation)
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
        
        private void PlantTreesInTerrainDesignation(TerrainDesignation designation, bool randomizeTreeOffset = false)
        {
            var treeProto = _protosDb.First<TreeProto>(x => x.Id == Ids.Trees.FirTree);
            designation.ForEachTile((TerrainTile tile, HeightTilesF f) =>
            {
                if (!designation.ManagedByTowers.IsEmpty())
                {
                    return;
                }
                
                var putTheTreeHere =randomizeTreeOffset ? tile.CenterTile2f  : tile.CenterTile2f + _treeManager.GetRandomPlantingOffset((Fix32) 1 / Fix32.Sqrt2);
                
                var isShitInTheWay = IsShitInTheWay(tile, putTheTreeHere, treeProto);
                if (isShitInTheWay)
                {
                    return;
                }

                HeightTilesF height = _terrainManager.GetHeight(tile.CenterTile2f);
                Percent baseScale = Percent.Hundred;
                var twelveYearOldTreeTicks = _calendar.RealTime.Ticks - Duration.FromYears(12).Ticks;
                _treeManager.TryAddTree(new TreeData(treeProto.Value, putTheTreeHere, height, baseScale, twelveYearOldTreeTicks, AngleSlim.Zero));
                
            });
            
        }

        private bool IsShitInTheWay(TerrainTile tile, Tile2f putTheTreeHere, Option<TreeProto> treeProto)
        {
            return _terrainManager.IsBlockingBuildings(tile.CoordAndIndex.Index) 
                   || _terrainOccupancyManager.IsOccupied(tile.CoordAndIndex.Index) 
                   || !_treeManager.IsValidTileForPlanting(putTheTreeHere.Tile2i, treeProto.Value.SpacingToOtherTree)
                   || _terrainManager.IsOffLimits(tile.CoordAndIndex.Index)
                   || _terrainManager.IsOceanOrOnMapBoundary(tile.CoordAndIndex.Index);
        }

        public void CompleteAllMiningDesignations(bool ignoreMineTowerDesignations, bool disablePhysicsOnMinedTiles)
        {
            var miningTerrainDesignations = _terrainMiningManager.MiningDesignations
                .Where(x => x.IsNotFulfilled);

            foreach (var designation in miningTerrainDesignations)
            {
                if (!designation.ManagedByTowers.IsEmpty() && ignoreMineTowerDesignations) continue;

                HarvestTreesInTerrainDesignation(designation);
                SetAllTileHeightToMatchDesignation(designation, disablePhysicsOnMinedTiles);
            }
        }

        public void CompleteAllDumpingDesignationsWithProduct(ProductProto.ID looseMaterialProductId, bool disablePhysicsOnDumpedTiles,
            bool ignoreMineTowerDesignations = true)
        {
            var dumpingDesignations = _terrainDumpingManager.DumpingDesignations
                .Where(x => x.IsNotFulfilled);

            foreach (var designation in dumpingDesignations)
            {
                if (!designation.ManagedByTowers.IsEmpty() && ignoreMineTowerDesignations) continue;

                HarvestTreesInTerrainDesignation(designation);
                DumpMaterialToDesignationHeight(designation, looseMaterialProductId, disablePhysicsOnDumpedTiles);
            }
        }

        public void ChangeTerrain(ProductProto.ID looseMaterialProductId, bool ignoreMineTowerDesignations = true)
        {
            var terrainMaterialThickness = LooseMaterialProductIdToTerrainMaterialThickness(looseMaterialProductId);

            var dumpingDesignations = _terrainDumpingManager.DumpingDesignations;

            foreach (var designation in dumpingDesignations)
            {
                if (!designation.ManagedByTowers.IsEmpty() && ignoreMineTowerDesignations) continue;

                designation.ForEachTile((tile, f) =>
                {
                    _terrainManager.ConvertMaterialInFirstLayer(
                        tile.CoordAndIndex, terrainMaterialThickness.Material.SlimId, ThicknessTilesF.One, ThicknessTilesF.One);

                    //This replaces all the way down to forever
                    //_terrainManager.DumpMaterial_NoHeightChange(tile.CoordAndIndex, terrainMaterialThickness.AsSlim());
                });
            }
        }

        private void DumpMaterialToDesignationHeight(TerrainDesignation terrainDesignation, ProductProto.ID looseMaterialProductId, bool disablePhysicsOnDumpedTiles)
        {
            var terrainMaterialThickness = LooseMaterialProductIdToTerrainMaterialThickness(looseMaterialProductId);

            terrainDesignation.ForEachTile((tile, f) =>
            {
                _terrainManager.DumpMaterialUpToHeight(tile.CoordAndIndex, terrainMaterialThickness.AsSlim, f);
                if (disablePhysicsOnDumpedTiles) _terrainManager.StopTerrainPhysicsSimulationAt(tile.CoordAndIndex);
            });
        }

        private void SetAllTileHeightToMatchDesignation(TerrainDesignation terrainDesignation, bool disablePhysicsOnDumpedTiles)
        {
            terrainDesignation.ForEachTile((tile, f) =>
            {
                tile.SetHeight(GetTargetHeight(terrainDesignation, tile.TileCoord));
                if (disablePhysicsOnDumpedTiles) _terrainManager.StopTerrainPhysicsSimulationAt(tile.CoordAndIndex);
            });
        }

        private HeightTilesF GetTargetHeight(TerrainDesignation terrainDesignation, Tile2i position)
        {
            return terrainDesignation.ContainsPosition(position) ? terrainDesignation.GetTargetHeightAt(position) : HeightTilesF.MinValue;
        }

        private TerrainMaterialThickness LooseMaterialProductIdToTerrainMaterialThickness(ProductProto.ID looseMaterialProductId)
        {
            var looseProductProto = _protosDb.First<LooseProductProto>(x => x.Id == looseMaterialProductId);
            var looseProductQuantity = new LooseProductQuantity(looseProductProto.Value, Quantity.MaxValue);
            var terrainMaterialThickness = looseProductQuantity.ToTerrainThickness();
            return terrainMaterialThickness;
        }
    }
}