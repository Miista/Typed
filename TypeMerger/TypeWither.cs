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
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            
            return new WithBuilder<T>(instance);
        }

        internal static T With<T>(T instance, params PropertyValue[] properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var constructorInfo = ReflectionUtils.GetSuitableConstructor<T>();

            var propertyDictionary = properties.ToDictionary(tuple => tuple.PropertyName.ToLowerInvariant(), tuple => tuple.Value);
            
            return constructorInfo == null
                ? WithByProperty(instance, propertyDictionary)
                : WithByConstructor(instance, propertyDictionary, constructorInfo);
        }

        private static TInstance WithByConstructor<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> properties,
            ConstructorInfo constructorInfo)
        {
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();

            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                existingProperties.TryGetValue(parameter.Name.ToLowerInvariant(), out var leftProperty);
                var originalValue = leftProperty?.GetValue(instance);
                
                var hasNewValue = properties.TryGetValue(parameter.Name.ToLowerInvariant(), out var newValue);

                var value = hasNewValue ? newValue : originalValue;
                
                parameters.Add(value);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) is TInstance
                ? (TInstance) constructorInfo.Invoke(parameters.ToArray())
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");

            return constructedInstance;
        }

        private static TInstance WithByProperty<TInstance>(
            TInstance instance, 
            IReadOnlyDictionary<string, object> properties)
        {
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();

            var constructedInstance = Activator.CreateInstance<TInstance>();
            
            foreach (var parameter in existingProperties)
            {
                if (!parameter.Value.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{parameter.Key}' cannot be written to.");
                }
                
                existingProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var existingProperty);
                var originalValue = existingProperty?.GetValue(instance);
                var hasNewValue = properties.TryGetValue(parameter.Key.ToLowerInvariant(), out var newValue);

                var value = hasNewValue ? newValue : originalValue;

                parameter.Value.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }
    }
}