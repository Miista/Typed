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

        public T Construct(T instance, IDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var valueResolver = new DependentValueResolver<T>(instance);
            
            // 1. Construct instance of T (and set properties via constructor)
            var (constructedInstance, remainingPropertiesAfterCtor) = WithByConstructor(instance, properties, _constructorInfo, valueResolver);
            
            // 2. Set new properties via property setters
            var (enrichedInstance, remainingPropertiesAfterPropSet) = EnrichByProperty(constructedInstance, remainingPropertiesAfterCtor, valueResolver);

            if (remainingPropertiesAfterPropSet.Count != 0)
            {
                throw new InvalidOperationException($"Error creating instance of type '{typeof(T)}': There are still properties to set but no way to set them.");
            }
            
            // 3. Copy remaining properties
            var copyProperties = GetCopyProperties(_constructorInfo, properties);
            var enrichedInstanceWithCopiedProperties = CopyProperties(instance, enrichedInstance, copyProperties);
            
            return enrichedInstanceWithCopiedProperties;
        }

        private static (TInstance Instance, IDictionary<string, object> RemainingProperties) WithByConstructor<TInstance>(
            TInstance instance,
            IDictionary<string, object> newProperties,
            ConstructorInfo constructorInfo,
            DependentValueResolver<TInstance> dependentValueResolver)
        {
            var remainingProperties = new Dictionary<string, object>(newProperties);
            var parameters = BuildParameters(remainingProperties, constructorInfo, instance, newProperties, dependentValueResolver);

            var constructedInstance = constructorInfo.Invoke(parameters) is TInstance castedInstance
                ? castedInstance
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");

            return (constructedInstance, remainingProperties);
        }
        
        private static object[] BuildParameters<TInstance>(
            IDictionary<string, object> remainingProperties,
            ConstructorInfo constructorInfo,
            TInstance instance,
            IDictionary<string, object> newProperties,
            DependentValueResolver<TInstance> dependentValueResolver)
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
                        ? dependentValueResolver.Resolve(dependentValue, existingProperty)
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
        /// Mutates <paramref name="instance"/>, setting all properties in <paramref name="propertiesToSet"/>.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="propertiesToSet">The properties to set.</param>
        /// <param name="dependentValueResolver">The value resolver.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>A mutated instance.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        private static (TInstance Instance, IDictionary<string, object> RemainingProperties) EnrichByProperty<TInstance>(
            TInstance instance,
            IDictionary<string, object> propertiesToSet,
            DependentValueResolver<TInstance> dependentValueResolver)
        {
            var existingProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TInstance>();
            var remainingProperties = new Dictionary<string, object>(propertiesToSet);

            foreach (var property in propertiesToSet)
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
                    ? dependentValueResolver.Resolve(dependentValue, existingProperty)
                    : property.Value;
                
                existingProperty.SetValue(instance, value);
                remainingProperties.Remove(property.Key);
            }

            return (instance, remainingProperties);
        }

        private static IEnumerable<PropertyInfo> GetCopyProperties(
            ConstructorInfo constructorInfo,
            IDictionary<string, object> excludeProperties)
        {
            var publicProperties = TypeUtils.GetPropertyDictionary<T>();
            
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