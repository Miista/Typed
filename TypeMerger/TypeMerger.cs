using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeMerger
{
    public class TypeMerger
    {
        public static TDestination Merge<TDestination, TLeft, TRight>(TLeft left, TRight right)
            where TDestination : class
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            var constructorInfo = GetSuitableConstructor<TDestination>();

            return constructorInfo == null
                ? MergeByProperty<TDestination, TLeft, TRight>(left, right)
                : MergeByConstructor<TDestination, TLeft, TRight>(left, right, constructorInfo);
        }

        private static ConstructorInfo GetSuitableConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault(info => info.GetParameters().Length > 0);
        }

        private static TDestination MergeByConstructor<TDestination, TLeft, TRight>(
            TLeft left,
            TRight right,
            ConstructorInfo constructorInfo)
            where TDestination : class
        {
            var leftProperties = GetProperties<TLeft>();
            var rightProperties = GetProperties<TRight>();

            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                leftProperties.TryGetValue(parameter.Name.ToLowerInvariant(), out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                rightProperties.TryGetValue(parameter.Name.ToLowerInvariant(), out var rightProperty);
                var rightValue = rightProperty?.GetValue(right);

                var value = rightValue ?? leftValue;
                
                parameters.Add(value);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) as TDestination;

            return constructedInstance;
        }

        private static TDestination MergeByProperty<TDestination, TLeft, TRight>(
            TLeft left, 
            TRight right)
            where TDestination : class
        {
            var leftProperties = GetProperties<TLeft>();
            var rightProperties = GetProperties<TRight>();
            var destinationProperties = GetProperties<TDestination>();

            var constructedInstance = Activator.CreateInstance<TDestination>();
            
            foreach (var parameter in destinationProperties)
            {
                leftProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                rightProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var rightProperty);
                var rightValue = rightProperty?.GetValue(right);

                var value = rightValue ?? leftValue;
                
                var setMethod = parameter.Value.GetSetMethod();
                setMethod.Invoke(constructedInstance, new[] {value});
            }

            return constructedInstance;
        }

        private static Dictionary<string, PropertyInfo> GetProperties<T>() => typeof(T).GetProperties().ToDictionary(info => info.Name.ToLowerInvariant());
    }
}