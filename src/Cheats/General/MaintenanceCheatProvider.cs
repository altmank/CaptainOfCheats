using System;
using System.Reflection;
using CaptainOfCheats.Config;
using CaptainOfCheats.Extensions;
using CaptainOfCheats.Logging;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Maintenance;

namespace CaptainOfCheats.Cheats.General
{
    public class MaintenanceCheatProvider
    {
        private readonly MaintenanceManager _maintenanceManager;


        public MaintenanceCheatProvider(MaintenanceManager maintenanceManager)
        {
            _maintenanceManager = maintenanceManager;
        }
        
        public bool IsMaintenanceEnabled()
        {
            return !_maintenanceManager.IsMaintenanceDisabled();
        }

        public void ToggleMaintenance(bool enable)
        {
            _maintenanceManager.SetMaintenanceDisabled(!enable);
        }
    }
}