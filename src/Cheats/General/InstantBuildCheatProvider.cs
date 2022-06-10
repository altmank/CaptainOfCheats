using System;
using System.Reflection;
using CaptainOfCheats.Config;
using CaptainOfCheats.Logging;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Utils;

namespace CaptainOfCheats.Cheats.General
{
    public class InstantBuildCheatProvider : ICheatProvider
    {
        private readonly IInstaBuildManager _instaBuildManager;
        private readonly Mafi.Lazy<Lyst<ICheatCommandBase>> _lazyCheats;
        private FieldInfo _instantBuildProperty;

        public InstantBuildCheatProvider(IInstaBuildManager instaBuildManager)
        {
            _instaBuildManager = instaBuildManager;
            _lazyCheats = new Mafi.Lazy<Lyst<ICheatCommandBase>>(GetCheats);
        }

        public Lyst<ICheatCommandBase> Cheats => _lazyCheats.Value;

        private void SetInstantBuildAccessors()
        {
            if (!(_instantBuildProperty is null)) return;

            var instantBuildManager = typeof(CoreMod).Assembly.GetType("Mafi.Core.Utils.InstaBuildManager");
            if (instantBuildManager is null)
            {
                Logger.Log.Error("Unable to fetch the InstaBuildManager type.");
                throw new Exception("Unable to fetch the InstaBuildManager type.");
            }

            _instantBuildProperty = instantBuildManager.GetField("<IsInstaBuildEnabled>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private Lyst<ICheatCommandBase> GetCheats()
        {
            return new Lyst<ICheatCommandBase>
            {
                new CheatToggleCommand(
                    "Instant Mode",
                    ToggleInstantMode,
                    IsToggleEnabled)
                {
                    Tooltip =
                        "Set instant mode off (left) or on (right). Enables instant build, instant research, instant upgrades (shipyards, buildings, settlements, mines), instant vehicle construction, and instant repair when on."
                }
            };
        }

        private bool IsToggleEnabled()
        {
            SetInstantBuildAccessors();
            return (bool)_instantBuildProperty.GetValue(_instaBuildManager);
        }

        private void ToggleInstantMode(bool enable)
        {
            SetInstantBuildAccessors();
            _instantBuildProperty.SetValue(_instaBuildManager, enable);
        }
    }
}