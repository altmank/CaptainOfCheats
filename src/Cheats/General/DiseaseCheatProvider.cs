using CaptainOfCheats.Extensions;
using CaptainOfCheats.Logging;
using Mafi;

using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using Mafi.Core.Simulation;

namespace CaptainOfCheats.Cheats.General
{
    public class DiseaseCheatProvider
    {
        private readonly PopsHealthManager _popsHealthManager;
        private readonly ProtosDb _protosDb;
        public bool IsDiseaseDisabled = false;

        public DiseaseCheatProvider(PopsHealthManager popsHealthManager, ICalendar calendar, ProtosDb protosDb)
        {
            _popsHealthManager = popsHealthManager;
            _protosDb = protosDb;
            calendar.NewDay.AddNonSaveable(this, OnNewDay);
        }
        
        private void OnNewDay()
        {
            if (!IsDiseaseDisabled)
            {
                return;
            }
            
            if (_popsHealthManager.CurrentDisease == Option<DiseaseProto>.None) return;
            
            Logger.Log.Info($"Ending pop disease {_popsHealthManager.CurrentDisease.Value.Id.Value}");
            _popsHealthManager.EndCurrentDisease();
        }

        public void ToggleDisease(bool toggleVal)
        {
            this.IsDiseaseDisabled = !toggleVal;
        }

        public void GenerateDisease()
        {
            var disease = _protosDb.First<DiseaseProto>(x => x.Id == new Proto.ID("Disease5")).Value;
            _popsHealthManager.Call("startDisease", disease);
        }
    }
}