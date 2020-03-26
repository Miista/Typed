using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeMerger
{
    public class TypeWither
    {
        public static T With<T, TValue>(T @this, Expression<Func<T, TValue>> selector, TValue value) where T : class
        {
            var property = (PropertyInfo) ((MemberExpression) selector.Body).Member;

            return With(@this, (property.Name, value));
        }
        
        public static T With<T>(T @this, params (string PropertyName, object Value)[] properties) where T : class
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));

            var constructorInfo = GetSuitableConstructor<T>();

            var propertyDictionary = properties.ToDictionary(tuple => tuple.PropertyName.ToLowerInvariant(), tuple => tuple.Value);
            
            return constructorInfo == null
                ? WithByProperty<T>(@this, propertyDictionary)
                : WithByConstructor<T>(@this, propertyDictionary, constructorInfo);
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
            where TObject : class
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

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) as TObject;

            return constructedInstance;
        }

        private static TObject WithByProperty<TObject>(
            TObject left, 
            IReadOnlyDictionary<string, object> properties)
            where TObject : class
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