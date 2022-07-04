using System;
using System.Collections.Generic;
using System.Text;
using CaptainOfCheats.Extensions;
using CaptainOfCheats.Logging;
using Mafi;
using Mafi.Core;
using Mafi.Core.PropertiesDb;
using Mafi.Core.Vehicles;

namespace CaptainOfCheats.Cheats.Vehicles
{
    public class VehiclesCheatProvider
    {
        public enum TruckCapacityMultiplier
        {
            Reset,
            OneHundred,
            TwoHundred,
            FiveHundred
        }

        private readonly IPropertiesDb _propsDb;
        private readonly PropDiffPercent _trucksCapacityMultiplier100 = PropertyModifiers.Diff("COC_TrucksCapacityMultiplier100", Percent.Hundred);
        private readonly PropDiffPercent _trucksCapacityMultiplier200 = PropertyModifiers.Diff("COC_TrucksCapacityMultiplier200", Percent.FromRaw(200000));
        private readonly PropDiffPercent _trucksCapacityMultiplier500 = PropertyModifiers.Diff("COC_TrucksCapacityMultiplier500", Percent.FromRaw(500000));
        private readonly IVehiclesManager _vehiclesManager;

        public VehiclesCheatProvider(IVehiclesManager vehiclesManager, IPropertiesDb propsDb)
        {
            _vehiclesManager = vehiclesManager;
            _propsDb = propsDb;
        }

        public void ChangeVehicleLimit(int diff)
        {
            _vehiclesManager.IncreaseVehicleLimit(diff);
        }


        public void SetTruckCapacityMultiplier(TruckCapacityMultiplier multiplier)
        {
            var trucksCapacityMultiplier = _propsDb.GetProperty(IdsCore.PropertyIds.TrucksCapacityMultiplier);

            Logger.Log.Info($"Removing any existing COC truck capacity modifiers");
            var cocModifierOwnerNames = new List<string>() { _trucksCapacityMultiplier100.Owner, _trucksCapacityMultiplier200.Owner, _trucksCapacityMultiplier500.Owner };
            var propertyModifiers = trucksCapacityMultiplier.GetModifiers();
            foreach (var modifier in propertyModifiers)
            {
                if (cocModifierOwnerNames.Contains(modifier.Owner))
                {
                    Logger.Log.Info($"Found existing modifier {modifier.Owner} to remove");
                    trucksCapacityMultiplier.RemoveModifier(modifier);
                }
            }


            switch (multiplier)
            {
                case TruckCapacityMultiplier.OneHundred:
                    Logger.Log.Info($"Adding modifier {_trucksCapacityMultiplier100.Owner}");
                    trucksCapacityMultiplier.AddModifier(_trucksCapacityMultiplier100);
                    break;
                case TruckCapacityMultiplier.TwoHundred:
                    Logger.Log.Info($"Adding modifier {_trucksCapacityMultiplier200.Owner}");
                    trucksCapacityMultiplier.AddModifier(_trucksCapacityMultiplier200);
                    break;
                case TruckCapacityMultiplier.FiveHundred:
                    Logger.Log.Info($"Adding modifier {_trucksCapacityMultiplier500.Owner}");
                    trucksCapacityMultiplier.AddModifier(_trucksCapacityMultiplier500);
                    break;
                case TruckCapacityMultiplier.Reset:
                    //Do nothing on reset
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, null);
            }
        }
    }
}