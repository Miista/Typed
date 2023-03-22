using System;
using System.Reflection;

namespace Typesafe.Snapshots
{
    internal static class ReflectionHelper
    {
        public static MethodInfo MakeGenericMethod<T>(
            string methodName,
            BindingFlags bindingFlags,
            Type[] genericTypes
        )
        {
            return typeof(T)
                       ?.GetMethod(methodName, bindingFlags)
                       ?.MakeGenericMethod(genericTypes)
                   ?? throw new Exception($"Cannot make generic method '{methodName}'");
        }
    }
}