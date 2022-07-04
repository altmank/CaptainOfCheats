using System;
using System.Reflection;
using Mafi.Collections;
using Mafi.Core.PropertiesDb;

namespace CaptainOfCheats.Extensions
{
    public static class PropertyExtensions
    {
        public static Lyst<PropertyModifier<T>> GetModifiers<T>(this IProperty<T> property)
        {
            var propertyType = property.GetType();
            var propModifiers = propertyType.GetField("m_modifiers", BindingFlags.NonPublic | BindingFlags.Instance);

            var modifierValues = (Lyst<PropertyModifier<T>>)propModifiers?.GetValue(property);

            return modifierValues;
        }
        
    }
}