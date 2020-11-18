using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Utils
{
    internal static class ReflectionUtils
    {
        public static ConstructorInfo GetSuitableConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length > 0);
        }

        public static ConstructorInfo GetPartiallySatisfiedConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length > 0);
        }
        
        public static ConstructorInfo GetDefaultConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length == 0);
        }
        
        public static ConstructorInfo GetSatisfiedConstructor<T>(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length >= parameters.Count);
        }
        
        public static ConstructorInfo GetSatisfiedConstructor<T>(IEnumerable<PropertyInfo> parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length >= parameters.Count());
        }
    }
}