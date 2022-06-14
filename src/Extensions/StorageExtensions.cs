using System;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi.Core;
using Mafi.Core.Buildings.Storages;

namespace CaptainOfCheats.Extensions
{
    public static class StorageExtensions
    {

        public static void SetGodMode(this Storage storage, bool enable)
        {
            storage.Call("Cheat_SetGodMode", enable);
        }

        public static bool IsGodModeEnabled(this Storage storage)
        {

            var type = typeof(CoreMod).Assembly.GetType("Mafi.Core.Buildings.Storages.Storage");
            if (type is null)
            {
                Logger.Log.Error("Unable to fetch the Storage type.");
                throw new Exception("Unable to fetch the Storage type.");
            }

            var property = type.GetField("<IsGodModeEnabled>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)property.GetValue(storage);
        }

    }
}