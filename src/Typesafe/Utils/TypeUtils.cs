using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Utils
{
    internal static class TypeUtils
    {
        public static Dictionary<string, PropertyInfo> GetPropertyDictionary<T>() => 
            typeof(T)
                .GetProperties()
                .ToDictionary(info => info.Name);

    }
}