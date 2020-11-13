using System;
using System.Collections.Generic;
using System.Reflection;

namespace TypeWither
{
    internal static class ConstructorInfoExtensions
    {
        public static bool CanSatisfy(
            this ConstructorInfo constructorInfo,
            IReadOnlyDictionary<string, object> properties)
        {
            if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            return constructorInfo.GetParameters().Length == properties.Count;
        }
    }
}