using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeMerger
{
    public static class TypeWither
    {
        public static WithBuilder<T> GetBuilder<T>(T instance)
        {
            return new WithBuilder<T>(instance);
        }

        internal static T With<T>(T instance, params (string PropertyName, object Value)[] properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var constructorInfo = ReflectionUtils.GetSuitableConstructor<T>();

            var propertyDictionary = properties.ToDictionary(tuple => tuple.PropertyName.ToLowerInvariant(), tuple => tuple.Value);
            
            return constructorInfo == null
                ? WithByProperty(instance, propertyDictionary)
                : WithByConstructor(instance, propertyDictionary, constructorInfo);
        }

        private static TObject WithByConstructor<TObject>(
            TObject instance,
            IReadOnlyDictionary<string, object> properties,
            ConstructorInfo constructorInfo)
        {
            var existingProperties = GetProperties<TObject>();

            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                existingProperties.TryGetValue(parameter.Name.ToLowerInvariant(), out var leftProperty);
                var originalValue = leftProperty?.GetValue(instance);
                
                var hasNewValue = properties.TryGetValue(parameter.Name.ToLowerInvariant(), out var newValue);

                var value = hasNewValue ? newValue : originalValue;
                
                parameters.Add(value);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) is TObject
                ? (TObject) constructorInfo.Invoke(parameters.ToArray())
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TObject)}");

            return constructedInstance;
        }

        private static TObject WithByProperty<TObject>(
            TObject left, 
            IReadOnlyDictionary<string, object> properties)
        {
            var existingProperties = GetProperties<TObject>();

            var constructedInstance = Activator.CreateInstance<TObject>();
            
            foreach (var parameter in existingProperties)
            {
                if (!parameter.Value.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{parameter.Key}' cannot be written to.");
                }
                
                existingProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var existingProperty);
                var originalValue = existingProperty?.GetValue(left);
                var hasNewValue = properties.TryGetValue(parameter.Key.ToLowerInvariant(), out var newValue);

                var value = hasNewValue ? newValue : originalValue;

                parameter.Value.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }

        private static Dictionary<string, PropertyInfo> GetProperties<T>() => 
            typeof(T)
                .GetProperties()
                .ToDictionary(info => info.Name.ToLowerInvariant());
    }
}