using System.Collections.Generic;
using Typesafe.Kernel;

namespace Typesafe.Snapshots.Cloner
{
    internal class ComplexCloner<T> : ITypeCloner<T>
    {
        public T Clone(T instance)
        {
            var constructorInfo = TypeUtils.GetSuitableConstructor<T>();

            var typeBuilder = new TypeBuilder<T>(constructorInfo);
            var clone = typeBuilder.Construct(instance, new Dictionary<string, object>());

            return clone;
        }
    }
}