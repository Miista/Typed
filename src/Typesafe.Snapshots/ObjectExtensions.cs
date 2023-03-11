using System;
using System.Collections.Generic;
using System.Linq;

namespace Typesafe.Snapshots
{
    public static class ObjectExtensions
    {
        public static T GetSnapshot<T>(this T self)
        {
            var typeCloner = Get<T>();
            var clone = typeCloner.Clone(self);

            return clone;
        }

        private static ITypeCloner<T> Get<T>()
        {
            var primitiveTypes = new[]
            {
                typeof(int),
                typeof(short),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(long),
                typeof(double),
                typeof(float),
                typeof(bool),
                typeof(char),
                typeof(byte),
            };

            if (primitiveTypes.Contains(typeof(T))) return new PrimitiveCloner<T>();

            if (typeof(T) == typeof(string)) return new StringCloner() as ITypeCloner<T>;

            if (typeof(T) == typeof(Guid)) return new GuidCloner() as ITypeCloner<T>;
            
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                var first = typeof(T).GenericTypeArguments.First();
                var cloner = typeof(ListCloner<>).MakeGenericType(first).GetConstructors().First().Invoke(new object[0]) as ITypeCloner<T>;

                return cloner;
            }
            
            return new ComplexCloner<T>();
        }
    }
}