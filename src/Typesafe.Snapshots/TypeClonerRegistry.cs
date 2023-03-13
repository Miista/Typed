using System;
using System.Collections.Generic;
using Typesafe.Snapshots.Cloner;
using Typesafe.Snapshots.Cloner.Collections;

namespace Typesafe.Snapshots
{
    public interface ITypeClonerRegistry
    {
        bool TryGetCloner<T>(out ITypeCloner<T> cloner);
    }
    
    public class TypeClonerRegistry : ITypeClonerRegistry
    {
        private readonly Dictionary<Type, ITypeCloner> _cloners = new Dictionary<Type, ITypeCloner>();

        public TypeClonerRegistry()
        {
            RegisterTypeCloner(typeof(int), new PrimitiveCloner<int>());
            RegisterTypeCloner(typeof(short), new PrimitiveCloner<short>());
            RegisterTypeCloner(typeof(ushort), new PrimitiveCloner<ushort>());
            RegisterTypeCloner(typeof(uint), new PrimitiveCloner<uint>());
            RegisterTypeCloner(typeof(ulong), new PrimitiveCloner<ulong>());
            RegisterTypeCloner(typeof(long), new PrimitiveCloner<long>());
            RegisterTypeCloner(typeof(double), new PrimitiveCloner<double>());
            RegisterTypeCloner(typeof(float), new PrimitiveCloner<float>());
            RegisterTypeCloner(typeof(bool), new PrimitiveCloner<bool>());
            RegisterTypeCloner(typeof(char), new PrimitiveCloner<char>());
            RegisterTypeCloner(typeof(byte), new PrimitiveCloner<byte>());
            RegisterTypeCloner(typeof(string), new StringCloner());
            RegisterTypeCloner(typeof(Guid), new GuidCloner());
            
            RegisterTypeCloner(typeof(List<>), new WrapperTypeCloner(typeof(ListCloner<>)));
            RegisterTypeCloner(typeof(Queue<>), new WrapperTypeCloner(typeof(QueueCloner<>)));
            RegisterTypeCloner(typeof(Stack<>), new WrapperTypeCloner(typeof(StackCloner<>)));
            RegisterTypeCloner(typeof(Dictionary<,>), new WrapperTypeCloner(typeof(DictionaryCloner<,>)));
            // RegisterTypeCloner(typeof(Dictionary<,>), new Cloner());
            // RegisterTypeCloner(typeof(Queue<>), new Cloner());
            // RegisterTypeCloner(typeof(Stack<>), new Cloner());
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

        public bool TryGetCloner<T>(out ITypeCloner<T> cloner)
        {
            if (InternalTryGetCloner<T>(typeof(T), out cloner))
            {
                return true;
            }

            if (typeof(T).IsGenericType)
            {
                var genericType = typeof(T).GetGenericTypeDefinition();

                if (InternalTryGetCloner<T>(genericType, out cloner))
                {
                    return true;
                }
                
                // if (_cloners.TryGetValue(genericType, out matchingCloner))
                // {
                //     if (matchingCloner is ITypeClonerProvider provider)
                //     {
                //         cloner = provider.Resolve<T>();
                //     }
                //     else
                //     {
                //         cloner = matchingCloner as ITypeCloner<T>;
                //     }
                //     
                //
                //     if (cloner == null) throw new InvalidOperationException($"Found cloner for type '{typeof(T)}' is not an instance of {typeof(ITypeCloner<T>)}");
                //
                //     return true;
                // }
            }

            cloner = new ComplexCloner<T>();
            return true;
        }

        private bool InternalTryGetCloner<T>(Type type, out ITypeCloner<T> cloner)
        {
            if (_cloners.TryGetValue(type, out var matchingCloner))
            {
                if (matchingCloner is ITypeClonerProvider provider)
                {
                    cloner = provider.Resolve<T>();
                }
                else
                {
                    cloner = matchingCloner as ITypeCloner<T>;
                }

                if (cloner == null) throw new InvalidOperationException($"Found cloner for type '{typeof(T)}' is not an instance of {typeof(ITypeCloner<T>)}");
                
                return true;
            }

            cloner = null;
            return false;
        }

        public void RegisterTypeCloner<T>(Type type, ITypeCloner<T> cloner)
        {
            lock (_cloners)
            {
                if (_cloners.TryGetValue(type, out _))
                {
                    throw new DuplicateRegistrationException(type);
                }
                
                _cloners.Add(type, cloner);
            }
        }
        
        public void RegisterTypeCloner(Type type, ITypeCloner cloner)
        {
            lock (_cloners)
            {
                if (_cloners.TryGetValue(type, out _))
                {
                    throw new DuplicateRegistrationException(type);
                }
                
                _cloners.Add(type, cloner);
            }
        }
    }

    public class DuplicateRegistrationException : Exception
    {
        public DuplicateRegistrationException(Type type) : base($"Cloner for type '{type.Name}' has already been registered") { }
    }
}