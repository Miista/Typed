using System;
using System.Collections.Generic;
using Typesafe.Snapshots.Cloner;
using Typesafe.Snapshots.Cloner.Collections;

namespace Typesafe.Snapshots.Registry
{
    public class TypeRegistryBuilder : ITypeRegistryBuilder
    {
        private readonly Dictionary<Type, ITypeCloner> _cloners = new Dictionary<Type, ITypeCloner>();
        private readonly Dictionary<Type, ITypeClonerProvider> _providers = new Dictionary<Type, ITypeClonerProvider>();

        public TypeRegistryBuilder()
        {
            // Primitives
            RegisterCloner(typeof(bool), new PrimitiveCloner<bool>());
            RegisterCloner(typeof(byte), new PrimitiveCloner<byte>());
            RegisterCloner(typeof(char), new PrimitiveCloner<char>());
            RegisterCloner(typeof(double), new PrimitiveCloner<double>());
            RegisterCloner(typeof(float), new PrimitiveCloner<float>());
            RegisterCloner(typeof(int), new PrimitiveCloner<int>());
            RegisterCloner(typeof(long), new PrimitiveCloner<long>());
            RegisterCloner(typeof(sbyte), new PrimitiveCloner<sbyte>());
            RegisterCloner(typeof(short), new PrimitiveCloner<short>());
            RegisterCloner(typeof(ushort), new PrimitiveCloner<ushort>());
            RegisterCloner(typeof(uint), new PrimitiveCloner<uint>());
            RegisterCloner(typeof(ulong), new PrimitiveCloner<ulong>());
            
            // Other
            RegisterCloner(typeof(string), new StringCloner());
            RegisterCloner(typeof(Guid), new GuidCloner());
            
            // Collections
            RegisterProvider(typeof(List<>), new WrapperTypeCloner(typeof(ListCloner<>)));
            RegisterProvider(typeof(LinkedList<>), new WrapperTypeCloner(typeof(LinkedListCloner<>)));
            RegisterProvider(typeof(SortedList<,>), new WrapperTypeCloner(typeof(SortedListCloner<,>)));
            RegisterProvider(typeof(HashSet<>), new WrapperTypeCloner(typeof(HashSetCloner<>)));
            RegisterProvider(typeof(SortedSet<>), new WrapperTypeCloner(typeof(SortedSetCloner<>)));
            RegisterProvider(typeof(Queue<>), new WrapperTypeCloner(typeof(QueueCloner<>)));
            RegisterProvider(typeof(Stack<>), new WrapperTypeCloner(typeof(StackCloner<>)));
            RegisterProvider(typeof(Dictionary<,>), new WrapperTypeCloner(typeof(DictionaryCloner<,>)));
            RegisterProvider(typeof(SortedDictionary<,>), new WrapperTypeCloner(typeof(SortedDictionaryCloner<,>)));
            RegisterProvider(typeof(CloneableClonerProvider), new NonGenericWrapperTypeCloner(typeof(CloneableCloner<>)));
            RegisterProvider(typeof(CopyConstructorClonerProvider), new NonGenericWrapperTypeCloner(typeof(CopyConstructorCloner<>)));
        }

        private class NonGenericWrapperTypeCloner : ITypeClonerProvider
        {
            private readonly Type _type;

            public NonGenericWrapperTypeCloner(Type type)
            {
                _type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public ITypeCloner<T> Resolve<T>()
            {
                var genericType = _type.MakeGenericType(typeof(T));
                var typeCloner = Activator.CreateInstance(genericType) as ITypeCloner<T>;

                return typeCloner;
            }
        }
        
        private class WrapperTypeCloner : ITypeClonerProvider
        {
            private readonly Type _type;

            public WrapperTypeCloner(Type type)
            {
                _type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public ITypeCloner<T> Resolve<T>()
            {
                var genericArguments = typeof(T).GetGenericArguments();
                var genericType = _type.MakeGenericType(genericArguments);
                var typeCloner = Activator.CreateInstance(genericType) as ITypeCloner<T>;

                return typeCloner;
            }
        }
        
        public void RegisterProvider(Type type, ITypeClonerProvider provider)
        {
            if (_providers.TryGetValue(type, out _))
            {
                throw new DuplicateRegistrationException(type);
            }
            
            _providers.Add(type, provider);
        }

        public void RegisterCloner<T>(Type type, ITypeCloner<T> cloner)
        {
            if (_cloners.TryGetValue(type, out _))
            {
                throw new DuplicateRegistrationException(type);
            }
            
            _cloners.Add(type, cloner);
        }

        public ITypeRegistry Build() => new TypeRegistry(cloners: _cloners, providers: _providers);
    }
}