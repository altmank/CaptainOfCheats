using System;
using CaptainOfCheats.Logging;
using HarmonyLib;
using Mafi;
using Mafi.Core.Population;

namespace CaptainOfCheats.Cheats.General
{
    public class DiseaseCheatProvider
    {
        public bool IsDiseaseDisabled = false;
        public void ToggleDisease(bool diseaseIsEnabled)
        {
            DisableDiseaseHarmonyPatcher.Toggle(!diseaseIsEnabled);
            IsDiseaseDisabled = !diseaseIsEnabled;
        }

        private class DisableDiseaseHarmonyPatcher
        {
            private static readonly Harmony Harmony = new Harmony($"CaptainOfCheats.{nameof(DisableDiseaseHarmonyPatcher)}");
            public static bool Prefix_PopsHealthManager_startDisease(DiseaseProto disease)
            {
                Logger.Log.Info($"Preventing {disease.Id.Value} disease from starting");
                return false;
            }

            public static void Postfix_PopsHealthManager_updateDiseaseOnNewDay(PopsHealthManager __instance)
            {
                var popsHealthManager = __instance;

                if (popsHealthManager.CurrentDisease.IsNone)
                {
                    return;
                }

                var diseaseName = popsHealthManager.CurrentDisease.Value.Id.Value;

                Logger.Log.Info($"Found existing disease {diseaseName} that will be removed");
                Traverse.Create(popsHealthManager).Property<Option<DiseaseProto>>("CurrentDisease").Value = Option<DiseaseProto>.None;
                Traverse.Create(popsHealthManager).Field<int>("m_monthsSinceLastDisease").Value = 0;
                Logger.Log.Info($"Disease {diseaseName} removed");
            }

            public static void Toggle(bool isEnabled)
            {
                if (isEnabled)
                {
                    Logger.Log.Info($"Enabling cheat patches from {nameof(DisableDiseaseHarmonyPatcher)}");

                    var popsHealthMgrType = typeof(PopsHealthManager);
                    var patcherType = typeof(DisableDiseaseHarmonyPatcher);
                    Harmony.Patch(AccessTools.Method(popsHealthMgrType,
                            "startDisease"),
                        new HarmonyMethod(patcherType,
                            nameof(Prefix_PopsHealthManager_startDisease)));
                    Harmony.Patch(AccessTools.Method(popsHealthMgrType,
                            "updateDiseaseOnNewDay"),
                        new HarmonyMethod(patcherType,
                            nameof(Postfix_PopsHealthManager_updateDiseaseOnNewDay)));
                }
                else
                {
                    Logger.Log.Info($"Disabling cheat patches from {nameof(DisableDiseaseHarmonyPatcher)}");
                    Harmony.UnpatchAll(Harmony.Id);
                }
            }
        }
    }
}