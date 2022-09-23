using System;
using System.IO;
using System.Reflection;
using CaptainOfCheats.Cheats;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Logger = CaptainOfCheats.Logging.Logger;

namespace CaptainOfCheats
{
    public sealed class CaptainOfCheatsMod : IMod
    {
        public string Name => "CaptainOfCheats";
        public int Version => 1;
        public bool IsUiOnly => false;

        
        public static readonly string ModRootDirPath = new FileSystemHelper().GetDirPath(FileType.Mod, false);
        public static readonly string ModDirPath = Path.Combine(ModRootDirPath, "CaptainOfCheats");
        public static readonly string PluginDirPath = Path.Combine(ModDirPath, "Plugins");
        public CaptainOfCheatsMod()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModAssemblies;
        }
        
        public static Assembly ResolveModAssemblies(object sender, ResolveEventArgs args)
        {
            
            string assemblyPath = Path.Combine(PluginDirPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                Logger.Log.Error("Assembly cannot loaded from Plugins, Assembly not found " + assemblyPath);
                return null;
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                Logger.Log.Info("Assembly loaded from Plugins: " + assembly.FullName + " : " + assemblyPath);
                return assembly;
            }
            catch (Exception e)
            {
                Logger.Log.Error("Assembly cannot loaded from Plugins due to exception  " + assemblyPath);
                Logger.Log.Exception(e, e.ToString());
            }
            return null;
        }
        
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