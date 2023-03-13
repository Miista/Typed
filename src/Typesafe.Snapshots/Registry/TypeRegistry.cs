using System;
using System.Collections.Generic;
using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly IReadOnlyDictionary<Type, ITypeCloner> _cloners;
        private readonly IReadOnlyDictionary<Type, ITypeClonerProvider> _providers;

        public TypeRegistry(
            Dictionary<Type, ITypeCloner> cloners,
            Dictionary<Type, ITypeClonerProvider> providers
        )
        {
            _cloners = cloners ?? throw new ArgumentNullException(nameof(cloners));
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        public ITypeCloner<T> TryGetCloner<T>()
        {
            var requestedType = typeof(T);

            if (requestedType.IsGenericType)
            {
                var genericType = requestedType.GetGenericTypeDefinition();

                if (_providers.TryGetValue(genericType, out var clonerProvider))
                {
                    return clonerProvider.Resolve<T>()
                           ?? throw new InvalidOperationException(
                               $"Found cloner for type '{typeof(T)}' is not an instance of {typeof(ITypeCloner<T>)}"
                           );
                }
            }

            if (_cloners.TryGetValue(requestedType, out var matchingCloner))
            {
                return matchingCloner as ITypeCloner<T>
                       ?? throw new InvalidOperationException(
                           $"Found cloner for type '{typeof(T)}' is not an instance of {typeof(ITypeCloner<T>)}"
                       );
            }

            // Default to ComplexCloner
            return new ComplexCloner<T>();
        }
    }
}