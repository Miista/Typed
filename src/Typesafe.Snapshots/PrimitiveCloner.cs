using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    
    internal class DictionaryCloner<TKey, TValue> : ITypeCloner<Dictionary<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> Clone(Dictionary<TKey, TValue> instance)
        {
            var newInstance = new Dictionary<TKey, TValue>(instance.Count);

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newInstance.Add(snapshot.Key, snapshot.Value);
            }

            return newInstance;
        }
    }
    
    internal class QueueCloner<T> : ITypeCloner<Queue<T>>
    {
        public Queue<T> Clone(Queue<T> instance)
        {
            var newInstance = new Queue<T>(instance.Count);

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newInstance.Enqueue(snapshot);
            }

            return newInstance;
        }
    }
    
    internal class StackCloner<T> : ITypeCloner<Stack<T>>
    {
        public Stack<T> Clone(Stack<T> instance)
        {
            var newInstance = new Stack<T>(instance.Count);

            foreach (var v in instance.Reverse())
            {
                var snapshot = v.GetSnapshot();
                newInstance.Push(snapshot);
            }

            return newInstance;
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