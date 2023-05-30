using System;
using System.Reflection;
using CaptainOfCheats.Logging;
using Mafi;
using Mafi.Collections;
using Mafi.Core.PropertiesDb;

namespace CaptainOfCheats.Extensions
{
    public static class PropertyExtensions
    {
        public static Lyst<PropertyModifier<T>> GetModifiers<T>(this IProperty<T> property)
        {
            var propertyType = typeof(Property<>).MakeGenericType(typeof(Percent));
            var propModifiers = propertyType.GetField("m_modifiers", BindingFlags.NonPublic | BindingFlags.Instance);

            if (propModifiers == null)
            {
                Logger.Log.Info($"GetModifiers<T> unable to get m_modifiers field");
            }
            
            Logger.Log.Info($"GetModifiers<T> GetField got field {propModifiers.Name}");
            
            var modifierValues = (Lyst<PropertyModifier<T>>)propModifiers?.GetValue(property);

            if (modifierValues == null)
            {
                Logger.Log.Info($"GetModifiers<T> GetValue of field is null");
            }
            
            return modifierValues;
        }
        
    }
}