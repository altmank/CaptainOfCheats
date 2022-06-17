using System;
using System.Reflection;
using CaptainOfCheats.Config;
using CaptainOfCheats.Extensions;
using CaptainOfCheats.Logging;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Utils;

namespace CaptainOfCheats.Cheats.General
{
    public class InstantBuildCheatProvider
    {
        private readonly IInstaBuildManager _instaBuildManager;

        public InstantBuildCheatProvider(IInstaBuildManager instaBuildManager)
        {
            _instaBuildManager = instaBuildManager;
        }

        public bool IsInstantModeEnabled()
        {
            return _instaBuildManager.IsInstaBuildEnabled;
        }

        public void ToggleInstantMode(bool enable)
        {
            _instaBuildManager.SetInstaBuildEnabled(enable);
        }
    }
}