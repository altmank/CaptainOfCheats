using HarmonyLib;
using Mafi.Core.Utils;

namespace CaptainOfCheats.Extensions
{
    public static class InstaBuildManagerExtensions
    {
        private static Traverse<bool> InstaBuildManager_IsInstaBuildEnabled(IInstaBuildManager manager) => Traverse.Create(manager).Property<bool>("IsInstaBuildEnabled");

        public static void SetInstaBuildEnabled(this IInstaBuildManager instaBuildManager, bool enable)
        {
            InstaBuildManager_IsInstaBuildEnabled(instaBuildManager).Value = enable;
        }
    }
}