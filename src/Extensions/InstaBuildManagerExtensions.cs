using System;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi.Core;
using Mafi.Core.Utils;

namespace CaptainOfCheats.Extensions
{
    public static class InstaBuildManagerExtensions
    {

        public static bool IsInstaBuildModeEnabled(this IInstaBuildManager instaBuildManager)
        {
            var property = GetIsInstaBuildEnabledProperty();
            return (bool)property.GetValue(instaBuildManager);
        }

        private static FieldInfo GetIsInstaBuildEnabledProperty()
        {
            var type = typeof(CoreMod).Assembly.GetType("Mafi.Core.Utils.InstaBuildManager");
            if (type is null)
            {
                Logger.Log.Error("Unable to fetch the IInstaBuildManager type.");
                throw new Exception("Unable to fetch the IInstaBuildManager type.");
            }

            var property = type.GetField("<IsInstaBuildEnabled>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            return property;
        }

        public static void SetInstaBuildEnabled(this IInstaBuildManager instaBuildManager, bool enable)
        {
            var isInstaBuildEnabledProperty = GetIsInstaBuildEnabledProperty();
            isInstaBuildEnabledProperty.SetValue(instaBuildManager, enable);
        }
        
    }
}