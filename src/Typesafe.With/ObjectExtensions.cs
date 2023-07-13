using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With
{
    public static class ObjectExtensions
    {
        public static T With<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propertyPicker,
            Expression<Func<TProperty, TProperty>> propertyValueFactory
        )
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            var properties = new Dictionary<string, object>
            {
                {propertyName, new DependentValue(propertyValueFactory, instance)}
            };

            Validate<T>(propertyName);
            
            var constructor = TypeUtils.GetSuitableConstructor<T>();
            var builder = new UnifiedWithBuilder<T>(constructor);
            
            return builder.Construct(instance, properties);
        }

        /// <summary>
        /// Sets the value of the property selected by <paramref name="propertyPicker"/> to <paramref name="propertyValue"/>.
        /// </summary>
        /// <param name="instance">The instance whose property to update.</param>
        /// <param name="propertyPicker">An expression representing the property to update.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="instance"/> or <paramref name="propertyPicker"/> are null.</exception>
        public static T With<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propertyPicker,
            TProperty propertyValue
        )
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            var properties = new Dictionary<string, object>
            {
                {propertyName, propertyValue}
            };

            Validate<T>(propertyName);
            
            var constructor = TypeUtils.GetSuitableConstructor<T>();
            var builder = new UnifiedWithBuilder<T>(constructor);
            
            return builder.Construct(instance, properties);
        }

        private static void Validate<T>(string propertyName)
        {
            // Can we set the property via constructor?
            var hasConstructorParameter = HasConstructorParameter<T>(propertyName);
            
            if (hasConstructorParameter) return;
            
            // Can we set the property via property setter?
            var hasPropertySetter = HasPropertySetter<T>(propertyName);
            
            if (hasPropertySetter) return;

            // If we cannot do either, then there is no point in continuing.
            throw new InvalidOperationException(
                $"Error calling {nameof(With)} on type {typeof(T)}: Property '{propertyName.ToPropertyCase()}' cannot be set via constructor or property setter. You can fix this by making the property settable or adding it as a constructor parameter."
            );
        }

        private static bool HasPropertySetter<T>(string propertyName)
        {
            return TypeUtils.GetPropertyDictionary<T>().TryGetValue(propertyName, out var propertyInfo) && propertyInfo.CanWrite;
        }

        private static bool HasConstructorParameter<T>(string propertyName)
        {
            var constructorParameters = TypeUtils.GetSuitableConstructor<T>().GetParameters();
            
            // Can we find a matching constructor parameter?
            var hasConstructorParameter = constructorParameters
                .Any(info => string.Equals(info.Name, propertyName, StringComparison.Ordinal));
            
            if (hasConstructorParameter) return true;

            // Can we find a matching constructor parameter if we lowercase both parameter and property name?
            var hasConstructorParameterByLowercase = constructorParameters
                .Any(info => string.Equals(info.Name, propertyName, StringComparison.InvariantCultureIgnoreCase));

            if (hasConstructorParameterByLowercase) return true;

            return false;
        }
    }
}