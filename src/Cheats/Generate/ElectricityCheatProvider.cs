using System;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi;
using Mafi.Core;
using Mafi.Core.Factory.ElectricPower;

namespace CaptainOfCheats.Cheats.Generate
{
    public class ElectricityCheatProvider
    {
        private readonly IElectricityManager _electricityManager;
        private FieldInfo _freeElectricityPerTickField;

        public ElectricityCheatProvider(IElectricityManager electricityManager)
        {
            _electricityManager = electricityManager;
        }

        private void SetAccessors()
        {
            if (!(_freeElectricityPerTickField is null)) return;
            var electricityManagerType = typeof(CoreMod).Assembly.GetType("Mafi.Core.Factory.ElectricPower.ElectricityManager");
            if (electricityManagerType is null)
            {
                Logger.Log.Error("Unable to fetch the ElectricityManager type.");
                throw new Exception("Unable to fetch the ElectricityManager type.");
            }

            _freeElectricityPerTickField = electricityManagerType.GetField("m_freeElectricityPerTick", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void SetFreeElectricity(int kw)
        {
            SetAccessors();
            _freeElectricityPerTickField.SetValue(_electricityManager, Electricity.FromKw(kw));
        }
    }
}