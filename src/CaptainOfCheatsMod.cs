using System;
using System.Reflection;
using CaptainOfCheats.Cheats;
using CaptainOfCheats.Logging;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;

namespace CaptainOfCheats
{
    public sealed class CaptainOfCheatsMod : IMod
    {
        public string Name => "CaptainOfCheats";
        public int Version => 1;
        public bool IsUiOnly => false;

        public void Initialize(DependencyResolver resolver, bool gameWasLoaded)
        {
            var version = GetVersion();

            Logger.Log.Info($"Running version v{version.Major}.{version.Minor}.{version.Build}");
        }

        private static Version GetVersion()
        {
            var assembly = Assembly.GetAssembly(typeof(CaptainOfCheats.CaptainOfCheatsMod));
            var gitVersionInformationType = assembly.GetType("GitVersionInformation");
            var major = Convert.ToInt32(gitVersionInformationType.GetField("Major").GetValue(null));
            var minor = Convert.ToInt32(gitVersionInformationType.GetField("Minor").GetValue(null));
            var patch = Convert.ToInt32(gitVersionInformationType.GetField("Patch").GetValue(null));
            return new Version(major, minor, patch);
        }

        public void ChangeConfigs(Lyst<IConfig> configs)
        { }

        public void RegisterPrototypes(ProtoRegistrator registrator)
        { }

        public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool wasLoaded)
        {
            depBuilder.RegisterAllTypesImplementing<ICheatProvider>(typeof(CaptainOfCheatsMod).Assembly);
        }
    }
}