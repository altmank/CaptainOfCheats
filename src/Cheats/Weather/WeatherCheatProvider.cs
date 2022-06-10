using System;
using System.Reflection;
using CaptainOfCheats.Config;
using CaptainOfCheats.Logging;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Environment;
using Mafi.Core.Prototypes;

namespace CaptainOfCheats.Cheats.Weather
{
    public class WeatherCheatProvider
    {
        private readonly Mafi.Lazy<Lyst<ICheatCommandBase>> _lazyCheats;
        private readonly ProtosDb _protosDb;
        private readonly IWeatherManager _weatherManager;
        private PropertyInfo _currentWeatherProperty;
        private FieldInfo _overrideDurationField;

        public WeatherCheatProvider(IWeatherManager weatherManager, ProtosDb protosDb)
        {
            _weatherManager = weatherManager;
            _protosDb = protosDb;
            _lazyCheats = new Mafi.Lazy<Lyst<ICheatCommandBase>>(GetCheats);
        }

        public Lyst<ICheatCommandBase> Cheats => _lazyCheats.Value;

        private void SetWeatherAccessors()
        {
            if (!(_currentWeatherProperty is null) && !(_overrideDurationField is null)) return;
            var weatherManagerType = typeof(CoreMod).Assembly.GetType("Mafi.Core.Environment.WeatherManager");
            if (weatherManagerType is null)
            {
                Logger.Log.Error("Unable to fetch the WeatherManager type.");
                throw new Exception("Unable to fetch the WeatherManager type.");
            }

            _currentWeatherProperty = weatherManagerType.GetProperty("CurrentWeather");
            _overrideDurationField = weatherManagerType.GetField("m_weatherOverrideDuration",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private Lyst<ICheatCommandBase> GetCheats()
        {
            return new Lyst<ICheatCommandBase>
            {
                new CheatButtonCommand(
                    "Reset weather",
                    () =>
                    {
                        SetWeatherAccessors();
                        SetWeather(Ids.Weather.Sunny, true);
                    }),
                new CheatButtonCommand(
                    "Sunny weather",
                    () =>
                    {
                        SetWeatherAccessors();
                        SetWeather(Ids.Weather.Sunny);
                    }),
                new CheatButtonCommand(
                    "Cloudy weather",
                    () =>
                    {
                        SetWeatherAccessors();
                        SetWeather(Ids.Weather.Cloudy);
                    }),

                new CheatButtonCommand(
                    "Rainy weather",
                    () =>
                    {
                        SetWeatherAccessors();
                        SetWeather(Ids.Weather.Rainy);
                    }),
                new CheatButtonCommand(
                    "Heavy rain weather",
                    () =>
                    {
                        SetWeatherAccessors();
                        SetWeather(Ids.Weather.HeavyRain);
                    })
            };
        }

        private void SetWeather(Proto.ID weatherTypeId, bool reset = false)
        {
            _currentWeatherProperty.SetValue(_weatherManager, _protosDb.First<WeatherProto>(x => x.Id == weatherTypeId).Value);
            _overrideDurationField.SetValue(_weatherManager, reset ? 0 : int.MaxValue);
        }
    }
}