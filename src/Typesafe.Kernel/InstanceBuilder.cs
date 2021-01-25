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
            var constructedInstance = ConstructInstance<T>(_valueResolver);
            
            // 2. Set new properties via property setters
            var enrichedInstance = EnrichByProperty(constructedInstance, _valueResolver);

            return enrichedInstance;
        }

        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all public non-constructor properties.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="valueResolver">The value resolver.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>A mutated instance.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        private static TInstance EnrichByProperty<TInstance>(
            TInstance instance,
            IValueResolver<T> valueResolver)
        {
            var instanceAsObject = (object) instance;
            var constructorParameters = GetConstructorParameters<TInstance>().Select(info => info.Name);
            
            var publicProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TInstance>();
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

            return (TInstance) instanceAsObject;
        }

        private static TInstance ConstructInstance<TInstance>(
            IValueResolver<T> valueResolver)
        {
            var constructorParameterValues = GetConstructorArguments<TInstance>(valueResolver);

            if (Activator.CreateInstance(typeof(TInstance), constructorParameterValues.ToArray()) is TInstance instance) return instance;

            throw new InvalidOperationException($"Cannot construct instance of type '{typeof(T).Name}'.");
        }

        private static object[] GetConstructorArguments<TInstance>(IValueResolver<T> valueResolver)
        {
            return GetConstructorParameters<TInstance>()
                .Select(info => info.Name)
                .Select(valueResolver.Resolve)
                .ToArray();
        }

        private static ParameterInfo[] GetConstructorParameters<TInstance>()
        {
            var constructorParameters = typeof(TInstance)
                                            ?.GetConstructors()
                                            ?.OrderByDescending(info => info.GetParameters().Length)
                                            ?.FirstOrDefault()
                                            ?.GetParameters()
                                        ?? new ParameterInfo[0];
            return constructorParameters;
        }
    }
}