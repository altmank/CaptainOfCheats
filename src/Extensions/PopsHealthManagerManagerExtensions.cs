using System;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi;
using Mafi.Core;
using Mafi.Core.Maintenance;
using Mafi.Core.Population;

namespace CaptainOfCheats.Extensions
{
    public static class PopsHealthManagerManagerExtensions
    {
        public static void EndCurrentDisease(this PopsHealthManager popsHealthManager)
        {
            var type = typeof(PopsHealthManager);
            var monthsSinceLastDisease = type.GetField("m_monthsSinceLastDisease", BindingFlags.NonPublic | BindingFlags.Instance);
            var currentDisease = type.GetField("<CurrentDisease>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

            currentDisease.SetValue(popsHealthManager, Option<DiseaseProto>.None);
            monthsSinceLastDisease.SetValue(popsHealthManager, 0);
        }
    }
}