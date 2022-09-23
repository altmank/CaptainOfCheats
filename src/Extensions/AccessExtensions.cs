using HarmonyLib;

namespace CaptainOfCheats.Extensions
{
    static class AccessExtensions
    {
        public static object Call(this object o, string methodName, params object[] args)
        {
            return Traverse.Create(o).Method(methodName).GetValue(args);
        }
    }
}