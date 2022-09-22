using System;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi.Core;
using Mafi.Core.Maintenance;

namespace CaptainOfCheats.Extensions
{
    public static class MaintenanceManagerExtensions
    {
        public static bool IsMaintenanceDisabled(this MaintenanceManager maintenanceManager)
        {
            var property = GetMaintenanceDisabledProperty();
            return (bool)property.GetValue(maintenanceManager);
        }

        private static FieldInfo GetMaintenanceDisabledProperty()
        {
            var type = typeof(CoreMod).Assembly.GetType("Mafi.Core.Maintenance.MaintenanceManager");
            if (type is null)
            {
                Logger.Log.Error("Unable to fetch the MaintenanceManager type.");
                throw new Exception("Unable to fetch the MaintenanceManager type.");
            }

            var property = type.GetField("m_maintenanceDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
            return property;
        }

        public static void SetMaintenanceDisabled(this MaintenanceManager maintenanceManager, bool enable)
        {
            var maintenanceDisabledProperty = GetMaintenanceDisabledProperty();
            maintenanceDisabledProperty.SetValue(maintenanceManager, enable);
        }
        
    }
}