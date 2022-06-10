using CaptainOfCheats.Config;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Vehicles;

namespace CaptainOfCheats.Cheats.General
{
    public class VehiclesCheatProvider : ICheatProvider
    {
        private readonly Lazy<Lyst<ICheatCommandBase>> _lazyCheats;
        private readonly IVehiclesManager _vehiclesManager;


        public VehiclesCheatProvider(IVehiclesManager vehiclesManager)
        {
            _vehiclesManager = vehiclesManager;
            _lazyCheats = new Lazy<Lyst<ICheatCommandBase>>(GetCheats);
        }

        public Lyst<ICheatCommandBase> Cheats => _lazyCheats.Value;

        private Lyst<ICheatCommandBase> GetCheats()
        {
            return new Lyst<ICheatCommandBase>
            {
                new CheatButtonCommand(
                    "Vehicle Limit Add 100",
                    () => _vehiclesManager.IncreaseVehicleLimit(100)),
                new CheatButtonCommand(
                    "Vehicle Limit Remove 100",
                    () => _vehiclesManager.IncreaseVehicleLimit(-100)),
                new CheatButtonCommand(
                    "Vehicle Limit Add 10",
                    () => _vehiclesManager.IncreaseVehicleLimit(10)),
                new CheatButtonCommand(
                    "Vehicle Limit Remove 10",
                    () => _vehiclesManager.IncreaseVehicleLimit(-10))
            };
        }
    }
}