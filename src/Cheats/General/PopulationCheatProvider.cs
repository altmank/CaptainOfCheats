using System;
using Mafi.Core.Buildings.Settlements;

namespace CaptainOfCheats.Cheats.General
{
    public class PopulationCheatProvider
    {
        private readonly SettlementsManager _settlementsManager;

        public PopulationCheatProvider(SettlementsManager settlementsManager)
        {
            _settlementsManager = settlementsManager;
        }

        public void ChangePopulation(int diff)
        {
            if (diff < 0)
            {
                _settlementsManager.RemovePopsAsMuchAs(Math.Abs(diff));
            }

            if (diff > 0)
            {
                _settlementsManager.AddPops(diff, PopsAdditionReason.Other);    
            }
            
        }

 
    }
}