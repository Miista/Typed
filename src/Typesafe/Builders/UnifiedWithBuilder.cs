using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Utils;

namespace Typesafe.Builders
{
    public class UnifiedWithBuilder<T> : IWithBuilder<T>
    {
        private readonly ConstructorInfo _constructorInfo;

        public UnifiedWithBuilder(ConstructorInfo constructorInfo)
        {
            _constructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        }

        public T Construct(T instance, IReadOnlyDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            // 1. Construct instance of T (and set properties via constructor)
            var (constructedInstance, remainingPropertiesAfterCtor) = WithByConstructor(instance, properties, _constructorInfo);
            
            // 2. Set new properties via property setters
            var (enrichedInstance, remainingPropertiesAfterPropSet) = EnrichByProperty(constructedInstance, remainingPropertiesAfterCtor);

            if (remainingPropertiesAfterPropSet.Count != 0)
            {
                throw new InvalidOperationException("There are still properties to set but no way to set them.");
            }
            
            // 3. Copy remaining properties
            var copyProperties = GetCopyProperties(_constructorInfo, properties);
            var enrichedInstanceWithCopiedProperties = CopyProperties(instance, enrichedInstance, copyProperties);
            
            return enrichedInstanceWithCopiedProperties;
        }
        
        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all properties in <paramref name="newProperties"/>.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="newProperties">The properties to set.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>A mutated instance.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        private static (TInstance Instance, IReadOnlyDictionary<string, object> RemainingProperties) EnrichByProperty<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties)
        {
            var existingProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TInstance>();
            var remainingProperties = new Dictionary<string, object>(newProperties.ToDictionary(pair => pair.Key, pair => pair.Value));

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
                remainingProperties.Remove(property.Key);
            }

            return (instance, remainingProperties);
        }
        
        private static T CopyProperties(T source, T destination, IEnumerable<PropertyInfo> properties)
        {
            foreach (var kvp in properties)
            {
                var value = kvp.GetValue(source);
                kvp.SetValue(destination, value);
            }

            return destination;
        }

        private static IEnumerable<PropertyInfo> GetCopyProperties(
            ConstructorInfo constructorInfo,
            IReadOnlyDictionary<string, object> excludeProperties)
        {
            var publicProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<T>();
            
            // Remove properties already set
            foreach (var parameter in excludeProperties.Select(kvp => kvp.Key))
            {
                publicProperties.Remove(parameter);
            }
            
            // Remove properties set via constructor
            foreach (var parameter in GetConstructorParameterNames())
            {
                publicProperties.Remove(parameter);
            }

            return publicProperties.Values;
            
            IEnumerable<string> GetConstructorParameterNames()
            {
                return constructorInfo
                    .GetParameters()
                    .Select(parameterInfo => parameterInfo.Name)
                    .ToList();
            }
        }

        private static (TInstance Instance, IReadOnlyDictionary<string, object> RemainingProperties) WithByConstructor<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties,
            ConstructorInfo constructorInfo)
        {
            var existingProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TInstance>();

            var remainingProperties = new Dictionary<string, object>(newProperties.ToDictionary(pair => pair.Key, pair => pair.Value));
            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                existingProperties.TryGetValue(parameter.Name, out var existingProperty);
                var originalValue = existingProperty?.GetValue(instance);
                var hasNewValue = newProperties.TryGetValue(parameter.Name, out var newValue);
                var value = hasNewValue ? newValue : originalValue;
                
                parameters.Add(value);
                remainingProperties.Remove(parameter.Name);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) is TInstance
                ? (TInstance) constructorInfo.Invoke(parameters.ToArray())
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");

            return (constructedInstance, remainingProperties);
        }
    }
}