using HarmonyLib;
using Mafi.Core.Buildings.Storages;

namespace CaptainOfCheats.Extensions
{
    public static class StorageExtensions
    {
        private static Traverse<bool> Storage_IsGodModeEnabled(Storage storage) => Traverse.Create(storage).Property<bool>("IsGodModeEnabled");

        public static void SetGodMode(this Storage storage, bool enable)
        {
            Storage_IsGodModeEnabled(storage).Value = enable;
        }

        public static bool IsGodModeEnabled(this Storage storage)
        {
            return Storage_IsGodModeEnabled(storage).Value;
        }
    }
}