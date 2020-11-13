using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Typed.With")]
[assembly: InternalsVisibleTo("Typed.Merge")]

namespace Type.Core
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
        
        public static ConstructorInfo GetSatisfiedConstructor<T>(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length == parameters.Count);
        }
    }
}