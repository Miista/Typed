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
            RegisterCloner(typeof(int), new PrimitiveCloner<int>());
            RegisterCloner(typeof(short), new PrimitiveCloner<short>());
            RegisterCloner(typeof(ushort), new PrimitiveCloner<ushort>());
            RegisterCloner(typeof(uint), new PrimitiveCloner<uint>());
            RegisterCloner(typeof(ulong), new PrimitiveCloner<ulong>());
            RegisterCloner(typeof(long), new PrimitiveCloner<long>());
            RegisterCloner(typeof(double), new PrimitiveCloner<double>());
            RegisterCloner(typeof(float), new PrimitiveCloner<float>());
            RegisterCloner(typeof(bool), new PrimitiveCloner<bool>());
            RegisterCloner(typeof(char), new PrimitiveCloner<char>());
            RegisterCloner(typeof(byte), new PrimitiveCloner<byte>());
            RegisterCloner(typeof(string), new StringCloner());
            RegisterCloner(typeof(Guid), new GuidCloner());
            
            RegisterProvider(typeof(List<>), new WrapperTypeCloner(typeof(ListCloner<>)));
            RegisterProvider(typeof(Queue<>), new WrapperTypeCloner(typeof(QueueCloner<>)));
            RegisterProvider(typeof(Stack<>), new WrapperTypeCloner(typeof(StackCloner<>)));
            RegisterProvider(typeof(Dictionary<,>), new WrapperTypeCloner(typeof(DictionaryCloner<,>)));
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