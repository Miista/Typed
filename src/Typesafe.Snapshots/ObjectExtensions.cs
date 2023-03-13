using System;
using System.Collections.Generic;
using System.Linq;
using Typesafe.Snapshots.Cloner;
using Typesafe.Snapshots.Cloner.Collections;

namespace Typesafe.Snapshots
{
    public static class ObjectExtensions
    {
        public static T GetSnapshot<T>(this T self)
        {
            if (object.ReferenceEquals(null, self)) return default(T);
            
            var typeClonerRegistry = new TypeClonerRegistry();
            if (typeClonerRegistry.TryGetCloner<T>(out var cloner))
            {
                var clone = cloner.Clone(self);

                return clone;
            }

            throw new Exception($"Cannot get snapshot of type '{typeof(T)}'");
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
            
            if (typeof(T).IsGenericType)
            {
                var genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                
                if (genericTypeDefinition == typeof(List<>))
                {
                    var first = typeof(T).GenericTypeArguments.First();
                    var cloner = typeof(ListCloner<>).MakeGenericType(first).GetConstructors().First().Invoke(new object[0]) as ITypeCloner<T>;

                    return cloner;
                }

                if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var first = typeof(T).GenericTypeArguments.First();
                    var second = typeof(T).GenericTypeArguments.Skip(1).First();
                    var cloner = typeof(DictionaryCloner<,>).MakeGenericType(first, second).GetConstructors().First().Invoke(new object[0]) as ITypeCloner<T>;

                    return cloner;
                }

                if (genericTypeDefinition == typeof(Queue<>))
                {
                    var first = typeof(T).GenericTypeArguments.First();
                    var cloner = typeof(QueueCloner<>).MakeGenericType(first).GetConstructors().First().Invoke(new object[0]) as ITypeCloner<T>;

                    return cloner;
                }
                
                if (genericTypeDefinition == typeof(Stack<>))
                {
                    var first = typeof(T).GenericTypeArguments.First();
                    var cloner = typeof(StackCloner<>).MakeGenericType(first).GetConstructors().First().Invoke(new object[0]) as ITypeCloner<T>;

                    return cloner;
                }
            }

            return new NonCloningCloner<T>(); //ComplexCloner<T>();
        }
    }
}