using System;
using System.Linq;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With.Lazy
{
    public static class LazyObjectExtensions
    {
        /// <summary>
        /// Starts a lazily evaluated sequence of with'ers.
        /// </summary>
        /// <param name="instance">The instance whose property to update.</param>
        /// <param name="propertyPicker">An expression representing the property to update.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <returns>A lazy sequence which will update the value of the property when applied.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="instance"/> or <paramref name="propertyPicker"/> are null.</exception>
        public static LazyInstancedWithSequence<T> With<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propertyPicker,
            TProperty propertyValue)
            where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            Validate<T>(propertyName);

            return new LazyInstancedWithSequence<T>(instance).With(propertyPicker, propertyValue);
        }

        /// <summary>
        /// Starts a lazily evaluated sequence of with'ers.
        /// </summary>
        /// <param name="instance">The instance whose property to update.</param>
        /// <param name="propertyPicker">An expression representing the property to update.</param>
        /// <param name="propertyValueFactory">A function taking in the current value and returning the new value.</param>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <returns>A lazy sequence which will update the value of the property when applied.</returns>
        /// <exception cref="ArgumentNullException">If either parameter is null.</exception>
        public static LazyInstancedWithSequence<T> With<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propertyPicker,
            Func<TProperty> propertyValueFactory)
            where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));
            if (propertyValueFactory == null) throw new ArgumentNullException(nameof(propertyValueFactory));

            var propertyName = propertyPicker.GetPropertyName();
            Validate<T>(propertyName);

            return new LazyInstancedWithSequence<T>(instance).With(propertyPicker, propertyValueFactory);
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
            throw new InvalidOperationException($"Property '{propertyName.ToPropertyCase()}' cannot be set via constructor or property setter.");
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