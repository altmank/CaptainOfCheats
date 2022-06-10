using System;
using System.Reflection;
using CaptainOfCheats.Config;
using CaptainOfCheats.Logging;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Maintenance;

namespace CaptainOfCheats.Cheats.General
{
    public class MaintenanceCheatProvider : ICheatProvider
    {
        private readonly Mafi.Lazy<Lyst<ICheatCommandBase>> _lazyCheats;
        private readonly MaintenanceManager _maintenanceManager;
        private FieldInfo _maintenanceDisabledField;


        public MaintenanceCheatProvider(MaintenanceManager maintenanceManager)
        {
            _maintenanceManager = maintenanceManager;
            _lazyCheats = new Mafi.Lazy<Lyst<ICheatCommandBase>>(GetCheats);
        }

        public Lyst<ICheatCommandBase> Cheats => _lazyCheats.Value;

        private void SetAccessors()
        {
            if (!(_maintenanceDisabledField is null)) return;
            var electricityManagerType = typeof(CoreMod).Assembly.GetType("Mafi.Core.Maintenance.MaintenanceManager");
            if (electricityManagerType is null)
            {
                Logger.Log.Error("Unable to fetch the MaintenanceManager type.");
                throw new Exception("Unable to fetch the MaintenanceManager type.");
            }

            _maintenanceDisabledField = electricityManagerType.GetField("m_maintenanceDisabled",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private Lyst<ICheatCommandBase> GetCheats()
        {
            return new Lyst<ICheatCommandBase>
            {
                new CheatToggleCommand(
                        "Maintenance",
                        ToggleMaintenance, IsToggleEnabled)
                    { Tooltip = "Set Maintenance off (left) or on (right). If on, then your settlement will consume maintenance resources. If off, all consumption of maintenance will stop." }
            };
        }

        private bool IsToggleEnabled()
        {
            SetAccessors();
            return !(bool)_maintenanceDisabledField.GetValue(_maintenanceManager);
        }

        private void ToggleMaintenance(bool enable)
        {
            SetAccessors();
            _maintenanceDisabledField.SetValue(_maintenanceManager, !enable);
        }
    }
}