using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Kernel;

namespace Typesafe.With
{
    internal class PropertySetter<T>
    {
        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all properties in <paramref name="newProperties"/>.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="newProperties">The properties to set.</param>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        public void SetProperties(
            T instance,
            IReadOnlyDictionary<string, object> newProperties
        )
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (newProperties == null) throw new ArgumentNullException(nameof(newProperties));
            
            var dependentValueResolver = new DependentValueResolver<T>(instance);

            SetProperties(instance, newProperties, dependentValueResolver);
        }

        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all properties in <paramref name="newProperties"/>.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="newProperties">The properties to set.</param>
        /// <param name="dependentValueResolver">The value resolver.</param>
        /// <returns>The properties remaining to be set.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IReadOnlyDictionary<string, object> SetProperties(
            T instance,
            IReadOnlyDictionary<string, object> newProperties,
            DependentValueResolver<T> dependentValueResolver
        )
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (newProperties == null) throw new ArgumentNullException(nameof(newProperties));
            if (dependentValueResolver == null) throw new ArgumentNullException(nameof(dependentValueResolver));
            
            return InternalSetProperties(instance, newProperties, dependentValueResolver);
        }

        private static IReadOnlyDictionary<string, object> InternalSetProperties(
            T instance,
            IReadOnlyDictionary<string, object> newProperties,
            DependentValueResolver<T> dependentValueResolver
        )
        {
            var existingProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<T>();
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

                var value = property.Value is DependentValue dependentValue
                    ? dependentValueResolver.Resolve(dependentValue, existingProperty)
                    : property.Value;
                
                existingProperty.SetValue(instance, value);
                remainingProperties.Remove(property.Key);
            }

            return remainingProperties;
        }
    }
}