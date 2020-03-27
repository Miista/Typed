using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeMerger
{
    public static class TypeWither
    {
        public static WithBuilder<T> GetBuilder<T>(T @this)
        {
            return new WithBuilder<T>(@this);
        }

        public static T With<T>(T @this, params (string PropertyName, object Value)[] properties)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));

            var constructorInfo = GetSuitableConstructor<T>();

            var propertyDictionary = properties.ToDictionary(tuple => tuple.PropertyName.ToLowerInvariant(), tuple => tuple.Value);
            
            return constructorInfo == null
                ? WithByProperty(@this, propertyDictionary)
                : WithByConstructor(@this, propertyDictionary, constructorInfo);
        }

        private static ConstructorInfo GetSuitableConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length > 0);
        }
        
        private static TObject WithByConstructor<TObject>(
            TObject left,
            IReadOnlyDictionary<string, object> properties,
            ConstructorInfo constructorInfo)
        {
            var existingProperties = GetProperties<TObject>();

            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                existingProperties.TryGetValue(parameter.Name.ToLowerInvariant(), out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                var hasRightValue = properties.TryGetValue(parameter.Name.ToLowerInvariant(), out var rightValue);

                var value = hasRightValue ? rightValue : leftValue;
                
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
                existingProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                var hasRightValue = properties.TryGetValue(parameter.Key.ToLowerInvariant(), out var rightValue);

                var value = hasRightValue ? rightValue : leftValue;
                
                var setMethod = parameter.Value.GetSetMethod();
                setMethod.Invoke(constructedInstance, new[] {value});
            }

            return constructedInstance;
        }

        private static Dictionary<string, PropertyInfo> GetProperties<T>() => typeof(T).GetProperties().ToDictionary(info => info.Name.ToLowerInvariant());
    }
}