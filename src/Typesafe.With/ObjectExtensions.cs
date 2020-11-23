using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With
{
    public static class ObjectExtensions
    {
        public static T With<T, TProperty>(this T instance, Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
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
            var hasConstructorParameter = TypeUtils.GetSuitableConstructor<T>()
                .GetParameters()
                .Select(info => info.Name)
                .Contains(propertyName);
            
            if (hasConstructorParameter) return;
            
            // Can we set the property via property setter?
            if (TypeUtils.GetPropertyDictionary<T>().TryGetValue(propertyName, out var propertyInfo) && propertyInfo.CanWrite) return;

            // If we cannot do either, then there is no point in continuing.
            throw new InvalidOperationException($"Property '{propertyName.ToPropertyCase()}' cannot be set via constructor or property setter.");
        }
    }
}