using CaptainOfCheats.Cheats;
using CaptainOfCheats.Logging;
using Mafi;
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
            Logger.Log.Info("Running version v9");
            Logger.Log.Info($"Built for game version 0.4.2a");
        }

        public void RegisterPrototypes(ProtoRegistrator registrator)
        { }

        public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool wasLoaded)
        {
            depBuilder.RegisterAllTypesImplementing<ICheatProvider>(typeof(CaptainOfCheatsMod).Assembly);
        }
    }
}