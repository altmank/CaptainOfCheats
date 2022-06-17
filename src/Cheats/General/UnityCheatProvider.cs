using Mafi;
using Mafi.Core;
using Mafi.Core.Population;

namespace CaptainOfCheats.Cheats.General
{
    public class UnityCheatProvider
    {
        private readonly UpointsManager _upointsManager;
        public UnityCheatProvider(UpointsManager upointsManager)
        {
            _upointsManager = upointsManager;
        }

        public void AddUnity(int points)
        {
            _upointsManager.GenerateUnity(IdsCore.UpointsCategories.FreeUnity, new Upoints(points));
        }
    }
}