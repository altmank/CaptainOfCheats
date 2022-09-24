using System;
using System.IO;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi.Base;
using Mafi.Core;
using Mafi.Localization;

namespace CaptainOfCheats
{
    public static class EarlyAssemblyInjector
    {
        private static readonly string ModRootDirPath = new FileSystemHelper().GetDirPath(FileType.Mod, false);
        private static readonly string ModDirPath = Path.Combine(ModRootDirPath, "CaptainOfCheats");
        private static readonly string PluginDirPath = Path.Combine(ModDirPath, "Plugins");
        
        /// <summary>
        /// Mafi.Localization.LocalizationManager.ScanForStaticLocStrFields method results in this static class being found
        /// via reading this field which causes static constructor to initialize at the same time that
        /// Mafi.Localization.LocalizationManager.ScanForStaticLocStrFields happens in the game load sequence.
        /// </summary>
        public static LocStr Dummy = Loc.Str(Ids.Buildings.BarrierStraight1.ToString() + "__desc", "dummy", "dummy");

        static EarlyAssemblyInjector()
        {
            Logger.Log.Info("Early assembly injector static constructor has been initialized");
            Logger.Log.Info("AssemblyResolve hook has been added");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModAssemblies;
        }

        private static Assembly ResolveModAssemblies(object sender, ResolveEventArgs args)
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
    }
}