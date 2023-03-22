using System;
using System.Linq;

namespace Typesafe.Kernel
{
    internal static class TypeExtensions
    {
        public static bool ImplementsInterface<TInterface>(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.GetInterfaces().OfType<TInterface>().Any();
        }
    }
}