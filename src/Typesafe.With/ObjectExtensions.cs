using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With
{
    internal class WithValueResolver<T> : IValueResolver<T>
    {
        private readonly IReadOnlyDictionary<string, object> _values;

        public WithValueResolver(T instance, string propertyName, object propertyValue)
        {
            _values = typeof(T)
                ?.GetProperties()
                ?.Select(info =>
                {
                    var value = string.Equals(propertyName, info.Name.ToParameterCase()) ? propertyValue : info.GetValue(instance);
                    
                    return new KeyValuePair<string, object>(info.Name.ToParameterCase(), value);
                })
                ?.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        public object Resolve(string parameterName)
        {
            var x = parameterName.ToParameterCase();
            var y = parameterName.ToPropertyCase();
            
            if (_values.TryGetValue(parameterName, out var valueByExactMatch))
            {
                return valueByExactMatch;
            }

            if (_values.TryGetValue(parameterName.ToParameterCase(), out var valueByParameterCase))
            {
                return valueByParameterCase;
            }

            if (_values.TryGetValue(parameterName.ToLowerInvariant(), out var valueByLowercased))
            {
                return valueByLowercased;
            }

            throw new Exception();
        }
    }
    
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

            var valueResolver = new WithValueResolver<T>(instance, propertyName, propertyValue);
            var instanceBuilder = new InstanceBuilder<T>(valueResolver);
            return instanceBuilder.Create();
            
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