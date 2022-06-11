using Mafi;
using Mafi.Core;
using Mafi.Core.Population;
using Mafi.Core.Simulation;
using Mafi.Serialization;

namespace CaptainOfCheats.Cheats.Generate
{
    public class UnityCheatProvider
    {
        private readonly UpointsManager _upointsManager;
        private Upoints _freeUnityPerMonth = Upoints.Zero;

        public UnityCheatProvider(UpointsManager upointsManager, ICalendar calendar)
        {
            _upointsManager = upointsManager;
            calendar.NewMonth.Add(this, OnNewMonth);
        }

        private void OnNewMonth()
        {
            _upointsManager.GenerateUnity(IdsCore.UpointsCategories.FreeUnity, _freeUnityPerMonth);
        }

        public void SetFreeUPoints(int uPoints)
        {
            _freeUnityPerMonth = new Upoints(uPoints);
        }
        
        public static void Serialize(UnityCheatProvider value, BlobWriter writer)
        {
        }
        
        public static UnityCheatProvider Deserialize(BlobReader reader)
        {
            return null;
        }
        
    }
}