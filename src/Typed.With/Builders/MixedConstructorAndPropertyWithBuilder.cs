using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type.Core;

namespace Typed.With.Builders
{
    /// <summary>
    /// An IWithBuilder which first constructs the instance using the constructor.
    /// Next, any parameter not initialized by the constructor is set using the property's setter (if available).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MixedConstructorAndPropertyWithBuilder<T> : ConstructorWithBuilder<T>, IWithBuilder<T>
    {
        private readonly ConstructorInfo _constructorInfo;

        public MixedConstructorAndPropertyWithBuilder(ConstructorInfo constructorInfo)
            : base(constructorInfo)
        {
            _constructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        }

        public new T Construct(T instance, IReadOnlyDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            
            var constructedInstance = base.Construct(instance, properties);
            var remainingProperties = GetParametersNotInConstructor(_constructorInfo, properties);
            
            return EnrichByProperty(constructedInstance, remainingProperties);
        }

        private static IReadOnlyDictionary<string, object> GetParametersNotInConstructor(
            ConstructorInfo constructorInfo,
            IReadOnlyDictionary<string, object> properties)
        {
            var constructorParameters = constructorInfo
                .GetParameters()
                .Select(parameterInfo => parameterInfo.Name.ToLowerInvariant())
                .ToLookup(parameterName => parameterName);

            var propertiesCoveredByConstructor = properties
                .Where(kvp => constructorParameters.Contains(kvp.Key));
                
            return properties
                .Except(propertiesCoveredByConstructor)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        private static TInstance EnrichByProperty<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties)
        {
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();

            foreach (var property in newProperties)
            {
                if (!existingProperties.TryGetValue(property.Key, out var existingProperty))
                {
                    throw new InvalidOperationException($"Cannot find property with name '{property.Key}'.");
                }

                if (!existingProperty.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{property.Key}' cannot be written to.");
                }
                
                existingProperty.SetValue(instance, property.Value);
            }

            return instance;
        }
    }
}