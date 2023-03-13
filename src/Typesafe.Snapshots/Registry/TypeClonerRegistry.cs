using System;
using System.Collections.Generic;
using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public class TypeClonerRegistry : ITypeClonerRegistry
    {
        private readonly IReadOnlyDictionary<Type, ITypeCloner> _cloners;
        private readonly IReadOnlyDictionary<Type, ITypeClonerProvider> _providers;

        public TypeClonerRegistry(Dictionary<Type, ITypeCloner> cloners, Dictionary<Type, ITypeClonerProvider> providers)
        {
            _cloners = cloners ?? throw new ArgumentNullException(nameof(cloners));
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
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
                
                if (_providers.TryGetValue(genericType, out var clonerProvider))
                {
                    cloner = clonerProvider.Resolve<T>();

                    return true;
                }

                if (InternalTryGetCloner<T>(genericType, out cloner))
                {
                    return true;
                }
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
    }
}