using Mafi;
using Mafi.Core;
using Mafi.Core.Population;
using Mafi.Core.Simulation;

namespace CaptainOfCheats.Cheats.Generate
{
    public class UnityCheatProvider
    {
        private readonly UpointsManager _upointsManager;
        private Upoints _freeUnityPerMonth = Upoints.Zero;

        public UnityCheatProvider(UpointsManager upointsManager, ICalendar calendar)
        {
            _upointsManager = upointsManager;
            calendar.NewMonth.AddNonSaveable(this, OnNewMonth);
        }

        private void OnNewMonth()
        {
            _upointsManager.GenerateUnity(IdsCore.UpointsCategories.FreeUnity, _freeUnityPerMonth);
        }

        public void SetFreeUPoints(int uPoints)
        {
            _freeUnityPerMonth = new Upoints(uPoints);
        }
    }
}