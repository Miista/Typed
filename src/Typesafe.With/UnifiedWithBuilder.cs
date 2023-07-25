using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Kernel;

namespace Typesafe.With
{
    internal class UnifiedWithBuilder<T>
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
            var (constructedInstance, remainingPropertiesToSetAfterCtor) = WithByConstructor(instance, properties, _constructorInfo);
            
            // 2. Set new properties via property setters
            var (enrichedInstance, remainingPropertiesToSetAfterEnrichment) = EnrichByProperty(constructedInstance, remainingPropertiesToSetAfterCtor);

            if (remainingPropertiesToSetAfterEnrichment.Count > 0)
            {
                throw new InvalidOperationException($"Error creating instance of type '{typeof(T)}': There are still properties to set but no way to set them.");
            }
            
            // 3. Copy remaining properties
            var copyProperties = GetCopyProperties(_constructorInfo, properties);
            var enrichedInstanceWithCopiedProperties = CopyProperties(instance, enrichedInstance, copyProperties);
            
            return enrichedInstanceWithCopiedProperties;
        }

        private static (TInstance Instance, IReadOnlyDictionary<string, object> RemainingProperties) WithByConstructor<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties,
            ConstructorInfo constructorInfo)
        {
            var remainingProperties = newProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
            var parameters = BuildParameters(remainingProperties, constructorInfo, instance, newProperties);

            var constructedInstance = constructorInfo.Invoke(parameters);
            
            return constructedInstance is TInstance castedInstance
                ? (castedInstance, remainingProperties)
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");
        }
        
        private static object[] BuildParameters<TInstance>(
            IDictionary<string, object> remainingProperties,
            ConstructorInfo constructorInfo,
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties)
        {
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();
            var resolvedConstructorParameters = new List<object>();
            var constructorParameters = constructorInfo.GetParameters();
            
            foreach (var parameter in constructorParameters)
            {
                var (existingProperty, propertyName) = TryFindExistingProperty(parameter);
                var originalValue = existingProperty?.GetValue(instance);
                var hasNewValue = newProperties.TryGetValue(propertyName, out var newValue);
                var value = hasNewValue
                    ? newValue is DependentValue dependentValue
                        ? dependentValue.Resolve(existingProperty)
                        : newValue
                    : originalValue;

                resolvedConstructorParameters.Add(value);
                remainingProperties.Remove(propertyName);
            }

            return resolvedConstructorParameters.ToArray();
            
            (PropertyInfo ExistingProperty, string PropertyName) TryFindExistingProperty(ParameterInfo parameterInfo)
            {
                // Can we find a matching constructor parameter?
                if (existingProperties.TryGetValue(parameterInfo.Name, out var existingPropertyByExactMatch))
                {
                    return (existingPropertyByExactMatch, parameterInfo.Name);
                }

                // Can we find a matching constructor parameter if we lowercase both parameter and property name?
                var existingPropertyKey =
                    existingProperties.Keys.FirstOrDefault(key => string.Equals(key, parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    ?? throw new InvalidOperationException(
                        $"Error creating instance of type '{typeof(TInstance)}': Cannot find property for constructor parameter '{parameterInfo.Name}'. This is usually a sign that the constructor contains logic."
                    );

                var existingPropertyByLowercaseMatch = existingProperties[existingPropertyKey];
                
                return (existingPropertyByLowercaseMatch, existingPropertyKey);
            }
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
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();
            var remainingProperties = newProperties.ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var property in newProperties)
            {
                if (!existingProperties.TryGetValue(property.Key, out var existingProperty))
                {
                    throw new InvalidOperationException($"Error enriching instance of type '{typeof(TInstance)}': Cannot find property with name '{property.Key}'.");
                }

                if (!existingProperty.CanWrite)
                {
                    throw new InvalidOperationException($"Error enriching instance of type '{typeof(TInstance)}': Property '{property.Key}' cannot be written to. You can fix this by making the property settable.");
                }

                var value = property.Value is DependentValue dependentValue
                    ? dependentValue.Resolve(existingProperty)
                    : property.Value;
                
                existingProperty.SetValue(instance, value);
                remainingProperties.Remove(property.Key);
            }

            return (instance, remainingProperties);
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

            // Only take the properties which we can write to
            return publicProperties.Values.Where(info => info.CanWrite);
            
            IEnumerable<string> GetConstructorParameterNames()
            {
                return constructorInfo
                    .GetParameters()
                    .Select(parameterInfo => parameterInfo.Name)
                    .ToList();
            }
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
    }
}