using CaptainOfCheats.Config;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Input;
using Mafi.Core.World;

namespace CaptainOfCheats.Cheats.Shipyard
{
    public class FleetCheatProvider 
    {
        private readonly IInputScheduler _inputScheduler;

        public FleetCheatProvider(IInputScheduler inputScheduler)
        {
            _inputScheduler = inputScheduler;
        }

        public void FinishExploration()
        {
            _inputScheduler.ScheduleInputCmd(new ExploreFinishCheatCmd());
        }

        public void RepairFleet()
        {
            _inputScheduler.ScheduleInputCmd(new FleetRepairCheatCmd());
        }

    }
}