using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Utils;

namespace Typesafe.Merge
{
    public static class ObjectExtensions
    {
        public static TDestination Merge<TDestination, TLeft, TRight>(TLeft left, TRight right)
            where TDestination : class
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            var constructorInfo = ReflectionUtils.GetSuitableConstructor<TDestination>();
            if (constructorInfo != null)
                return MergeByConstructor<TDestination, TLeft, TRight>(left, right, constructorInfo);

            var defaultConstructorInfo = ReflectionUtils.GetDefaultConstructor<TDestination>();
            if (defaultConstructorInfo != null)
                return MergeByConstructor<TDestination, TLeft, TRight>(left, right, defaultConstructorInfo);

            return MergeByProperty<TDestination, TLeft, TRight>(left, right);
        }

        private static TDestination MergeByConstructor<TDestination, TLeft, TRight>(
            TLeft left,
            TRight right,
            ConstructorInfo constructorInfo)
            where TDestination : class
        {
            var leftProperties = GetPropertyDictionary<TLeft>();
            var rightProperties = GetPropertyDictionary<TRight>();

            var alreadySetProperties = new List<string>();
            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                leftProperties.TryGetValue(parameter.Name, out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                rightProperties.TryGetValue(parameter.Name, out var rightProperty);
                var rightValue = rightProperty?.GetValue(right);

                var value = rightValue ?? leftValue;
                
                parameters.Add(value);
                alreadySetProperties.Add(parameter.Name);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) as TDestination;

            var destinationProperties = GetPropertyDictionary<TDestination>();
            
            foreach (var pair in destinationProperties)
            {
                if (alreadySetProperties.Contains(pair.Key)) continue;
                
                leftProperties.TryGetValue(pair.Key, out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                rightProperties.TryGetValue(pair.Key, out var rightProperty);
                var rightValue = rightProperty?.GetValue(right);

                var value = rightValue ?? leftValue;
                
                pair.Value?.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }

        private static IDictionary<string, PropertyInfo> GetPropertyDictionary<TInstance>()
            => TypeUtils.GetPropertyDictionary<TInstance>().Select(LowercaseKey).ToDictionary(pair => pair.Key, pair => pair.Value);

        private static KeyValuePair<string, TValue> LowercaseKey<TValue>(KeyValuePair<string, TValue> pair)
        {
            var lowercasedKey = Lowercase(pair.Key);

            return new KeyValuePair<string, TValue>(lowercasedKey, pair.Value);

            string Lowercase(string s)
            {
                var firstLetter = s[0];
                var firstLetterInUppercase = char.ToLowerInvariant(firstLetter);
                var remainingString = s.Substring(1);
                    
                return firstLetterInUppercase + remainingString;
            }
        }
        
        private static TDestination MergeByProperty<TDestination, TLeft, TRight>(
            TLeft left, 
            TRight right)
            where TDestination : class
        {
            var leftProperties = GetPropertyDictionary<TLeft>();
            var rightProperties = GetPropertyDictionary<TRight>();
            var destinationProperties = GetPropertyDictionary<TDestination>();

            var constructedInstance = Activator.CreateInstance<TDestination>();

            var properties = new List<string>();

            foreach (var parameter in destinationProperties)
            {
                leftProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                rightProperties.TryGetValue(parameter.Key.ToLowerInvariant(), out var rightProperty);
                var rightValue = rightProperty?.GetValue(right);

                var value = rightValue ?? leftValue;
                
                var setMethod = parameter.Value.GetSetMethod();
                setMethod.Invoke(constructedInstance, new[] {value});
                
                properties.Add(parameter.Key);
            }

            foreach (var pair in leftProperties)
            {
                if (properties.Contains(pair.Key)) continue;
                
                leftProperties.TryGetValue(pair.Key, out var leftProperty);
                var leftValue = leftProperty?.GetValue(left);
                
                rightProperties.TryGetValue(pair.Key, out var rightProperty);
                var rightValue = rightProperty?.GetValue(right);

                var value = rightValue ?? leftValue;
                
                rightProperty?.SetValue(constructedInstance, value);
            }
            
            return constructedInstance;
        }
    }
}