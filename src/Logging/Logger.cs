using System;

namespace CaptainOfCheats.Logging
{
    public static class Logger
    {
        private const string LogPrefix = "[CaptainOfCheats]: ";

        public static class Log
        {
            public static void Info(string message)
            {
                Mafi.Log.Info($"{LogPrefix}{message}");
            }

            public static void Error(string message)
            {
                Mafi.Log.Error($"{LogPrefix}{message}");
            }

            public static void Exception(Exception e, string message)
            {
                Mafi.Log.Exception(e, $"{LogPrefix}{message}");
            }
        }
    }
}