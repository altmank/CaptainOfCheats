using Mafi;
using Mafi.Core.Factory.ComputingPower;

namespace CaptainOfCheats.Cheats.Generate
{
    public class ComputingCheatProvider
    {
        private readonly ComputingManager _computingManager;

        public ComputingCheatProvider(ComputingManager computingManager)
        {
            _computingManager = computingManager;
        }

        public void SetFreeCompute(int tFLops)
        {
            _computingManager.Cheat_AddFreeComputingPerTick(Computing.FromTFlops(tFLops));
        }
    }
}