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
        private readonly DisableDiseaseHarmonyPatcher _disableDiseaseHarmonyPatcher;

        public DiseaseCheatProvider()
        {
            _disableDiseaseHarmonyPatcher = new DisableDiseaseHarmonyPatcher();
        }
        public void ToggleDisease(bool diseaseIsEnabled)
        {
            _disableDiseaseHarmonyPatcher.Toggle(!diseaseIsEnabled);
            IsDiseaseDisabled = !diseaseIsEnabled;
        }

        private class DisableDiseaseHarmonyPatcher
        {
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

            public void Toggle(bool isEnabled)
            {
                var harmony = new Harmony($"CaptainOfCheats.{nameof(DisableDiseaseHarmonyPatcher)}");
                if (isEnabled)
                {
                    Logger.Log.Info($"Enabling cheat patches from {nameof(DisableDiseaseHarmonyPatcher)}");

                    var popsHealthMgrType = typeof(PopsHealthManager);
                    var patcherType = typeof(DisableDiseaseHarmonyPatcher);
                    harmony.Patch(AccessTools.Method(popsHealthMgrType,
                            "startDisease"),
                        new HarmonyMethod(patcherType,
                            nameof(Prefix_PopsHealthManager_startDisease)));
                    harmony.Patch(AccessTools.Method(popsHealthMgrType,
                            "updateDiseaseOnNewDay"),
                        new HarmonyMethod(patcherType,
                            nameof(Postfix_PopsHealthManager_updateDiseaseOnNewDay)));
                }
                else
                {
                    Logger.Log.Info($"Disabling cheat patches from {nameof(DisableDiseaseHarmonyPatcher)}");
                    harmony.UnpatchAll(harmony.Id);
                }
            }
        }
    }
}