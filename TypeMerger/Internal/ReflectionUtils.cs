using System.Linq;
using System.Reflection;

namespace TypeMerger.Internal
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
    }
}