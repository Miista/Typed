using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Kernel
{
    internal class InstanceBuilder<T>
    {
        private readonly IValueResolver<T> _valueResolver;

        public InstanceBuilder(IValueResolver<T> valueResolver)
        {
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
        }

        public T Construct()
        {
            // 1. Construct instance of T (and set properties via constructor)
            var constructedInstance = ConstructInstance(_valueResolver);
            
            // 2. Set new properties via property setters
            var enrichedInstance = EnrichByProperty(constructedInstance, _valueResolver);

            return enrichedInstance;
        }

        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all public non-constructor properties.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="valueResolver">The value resolver.</param>
        /// <returns>A mutated instance.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        private static T EnrichByProperty(T instance, IValueResolver<T> valueResolver)
        {
            var instanceAsObject = (object) instance;
            var constructorParameters = GetConstructorParameters().Select(info => info.Name);
            
            var publicProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<T>();
            foreach (var constructorParameter in constructorParameters)
            {
                publicProperties.Remove(constructorParameter);
            }
            
            foreach (var property in publicProperties)
            {
                if (!publicProperties.TryGetValue(property.Key, out var existingProperty))
                {
                    throw new InvalidOperationException($"Cannot find property with name '{property.Key}'.");
                }

                if (!existingProperty.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{property.Key.ToPropertyCase()}' cannot be written to.");
                }

                var value = valueResolver.Resolve(property.Key);
                existingProperty.SetValue(instanceAsObject, value, null);
            }

            return (T) instanceAsObject;
        }

        private static T ConstructInstance(IValueResolver<T> valueResolver)
        {
            var constructorParameterValues = GetConstructorArguments(valueResolver);

            if (Activator.CreateInstance(typeof(T), constructorParameterValues) is T instance) return instance;

            throw new InvalidOperationException($"Cannot construct instance of type '{typeof(T).Name}'.");
        }

        private static object[] GetConstructorArguments(IValueResolver<T> valueResolver)
        {
            return GetConstructorParameters()
                       ?.Select(info => info.Name)
                       ?.Select(valueResolver.Resolve)
                       ?.ToArray()
                   ?? new object[0];
        }

        private static ParameterInfo[] GetConstructorParameters()
        {
            var constructorParameters = typeof(T)
                                            ?.GetConstructors()
                                            ?.OrderByDescending(info => info.GetParameters().Length)
                                            ?.FirstOrDefault()
                                            ?.GetParameters()
                                        ?? new ParameterInfo[0];
            return constructorParameters;
        }
    }
}