using System;
using System.Linq;
using System.Reflection;

namespace Typesafe.Snapshots
{
    internal static class TypeExtensions
    {
        public static bool ImplementsInterface<TInterface>(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.GetInterfaces().OfType<TInterface>().Any();
        }

        public static bool HasCopyConstructor(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type
                .GetConstructors()
                .Any(info => info.IsCopyConstructor(type));
        }

        public static ConstructorInfo GetCopyConstructor(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type
                       .GetConstructors()
                       .FirstOrDefault(info => info.IsCopyConstructor(type))
                   ?? throw new Exception($"Type {type.Name} does not have a copy constructor");
        }
        
        private static bool IsCopyConstructor(this ConstructorInfo self, Type type)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (type == null) throw new ArgumentNullException(nameof(type));

            var parameters = self.GetParameters();

            return parameters.Length == 1 && parameters[0].ParameterType == type;
        }
    }
}