using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Kernel
{
    internal class UnifiedWithBuilderV2<T>
    {
        private readonly ConstructorInfo _constructorInfo;
        private readonly IValueResolver<T> _valueResolver;

        public UnifiedWithBuilderV2(ConstructorInfo constructorInfo, IValueResolver<T> valueResolver)
        {
            _constructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
        }

        public T Construct(T instance, IReadOnlyDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            // 1. Construct instance of T (and set properties via constructor)
            var (constructedInstance, remainingPropertiesAfterCtor) = ConstructInstance<T>(properties, _valueResolver);
            
            // 2. Set new properties via property setters
            var (enrichedInstance, remainingPropertiesAfterPropSet) = EnrichByProperty(constructedInstance, remainingPropertiesAfterCtor, _valueResolver);

            if (remainingPropertiesAfterPropSet.Count != 0)
            {
                throw new InvalidOperationException("There are still properties to set but no way to set them.");
            }
            
            // 3. Copy remaining properties
            var copyProperties = GetCopyProperties(_constructorInfo, properties);
            var enrichedInstanceWithCopiedProperties = CopyProperties(enrichedInstance, copyProperties, _valueResolver);
            
            return enrichedInstanceWithCopiedProperties;
        }

        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all properties in <paramref name="newProperties"/>.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="newProperties">The properties to set.</param>
        /// <param name="valueResolver">The value resolver.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>A mutated instance.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        private static (TInstance Instance, IReadOnlyDictionary<string, object> RemainingProperties) EnrichByProperty<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties,
            IValueResolver<T> valueResolver)
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

                var value = valueResolver.Resolve(property.Key);
                existingProperty.SetValue(instance, value);
                remainingProperties.Remove(property.Key);
            }

            return (instance, remainingProperties);
        }
        
        private static T CopyProperties(T destination, IEnumerable<PropertyInfo> properties, IValueResolver<T> valueResolver)
        {
            foreach (var kvp in properties)
            {
                var value = valueResolver.Resolve(kvp.Name);
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
            foreach (var parameter in GetConstructorParameterNames(constructorInfo))
            {
                publicProperties.Remove(parameter);
            }

            return publicProperties.Values.Where(info => info.CanWrite);
        }

        private static IEnumerable<string> GetConstructorParameterNames(ConstructorInfo constructorInfo)
        {
            return constructorInfo.GetParameters()
                .Select(parameterInfo => parameterInfo.Name)
                .ToList();
        }

        private static (TInstance Instance, IReadOnlyDictionary<string, object> RemainingProperties) ConstructInstance<TInstance>(
            IReadOnlyDictionary<string, object> newProperties,
            IValueResolver<T> valueResolver)
        {
            var constructorParameters = typeof(TInstance)
                                            ?.GetConstructors()
                                            ?.OrderByDescending(info => info.GetParameters().Length)
                                            ?.FirstOrDefault()
                                            ?.GetParameters()
                                        ?? new ParameterInfo[0];

            var remainingProperties = new Dictionary<string, object>(newProperties.ToDictionary(pair => pair.Key, pair => pair.Value));
            var constructorParameterValues = new List<object>();
            
            foreach (var constructorParameter in constructorParameters)
            {
                var parameterValue = valueResolver.Resolve(constructorParameter.Name);
                constructorParameterValues.Add(parameterValue);
                remainingProperties.Remove(constructorParameter.Name);
            }

            if (Activator.CreateInstance(typeof(TInstance), constructorParameterValues.ToArray()) is TInstance instance) return (instance, remainingProperties);

            throw new InvalidOperationException($"Cannot construct instance of type '{typeof(T).Name}'.");
        }
    }
}