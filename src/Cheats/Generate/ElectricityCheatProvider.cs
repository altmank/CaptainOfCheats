using HarmonyLib;
using Mafi;
using Mafi.Core.Factory.ElectricPower;

namespace CaptainOfCheats.Cheats.Generate
{
    public class ElectricityCheatProvider
    {
        private readonly IElectricityManager _electricityManager;

        public ElectricityCheatProvider(IElectricityManager electricityManager)
        {
            _electricityManager = electricityManager;
        }

        public void SetFreeElectricity(int kw)
        {
            Traverse.Create(_electricityManager).Field<Electricity>("m_freeElectricityPerTick").Value = Electricity.FromKw(kw);
        }
    }
}