using System;
using System.Collections.Generic;
using Typesafe.Kernel;

namespace Typesafe.Snapshots
{
    internal interface ITypeCloner<T>
    {
        T Clone(T instance);
    }
    
    internal class StringCloner : ITypeCloner<string>
    {
        public string Clone(string instance)
        {
            return string.Copy((string) instance);
        }
    }
    
    internal class PrimitiveCloner<T> : ITypeCloner<T>
    {
        public T Clone(T instance)
        {
            return (T) instance;
        }
    }

    internal class GuidCloner : ITypeCloner<Guid>
    {
        public Guid Clone(Guid instance)
        {
            return new Guid(instance.ToByteArray());
        }
    }

    internal class ListCloner<T> : ITypeCloner<List<T>>
    {
        public List<T> Clone(List<T> instance)
        {
            var newList = new List<T>(instance.Count);

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newList.Add(snapshot);
            }

            return newList;
        }
    }
    
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