using HarmonyLib;
using Mafi.Core.Maintenance;

namespace CaptainOfCheats.Extensions
{
    public static class MaintenanceManagerExtensions
    {
        private static Traverse<bool> MaintenanceManager_m_maintenanceDisabled(MaintenanceManager maintenanceManager) => Traverse.Create(maintenanceManager).Field<bool>("m_maintenanceDisabled");
        public static bool IsMaintenanceDisabled(this MaintenanceManager maintenanceManager)
        {
            return MaintenanceManager_m_maintenanceDisabled(maintenanceManager).Value;
        }

        public static void SetMaintenanceDisabled(this MaintenanceManager maintenanceManager, bool enable)
        {
            MaintenanceManager_m_maintenanceDisabled(maintenanceManager).Value = enable;
        }
    }
}