using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Utils;

namespace Typesafe.Merge
{
    public static class ObjectExtensions
    {
        private class ValueResolver<TLeft, TRight>
        {
            private readonly TLeft _leftInstance;
            private readonly TRight _rightInstance;
            private readonly IReadOnlyDictionary<string, PropertyInfo> _leftProperties;
            private readonly IReadOnlyDictionary<string, PropertyInfo> _rightProperties;

            public ValueResolver(TLeft leftInstance, TRight rightInstance)
            {
                _leftInstance = leftInstance;
                _rightInstance = rightInstance;
                _leftProperties = GetPropertyDictionary<TLeft>();
                _rightProperties = GetPropertyDictionary<TRight>();
            }

            public object Resolve(string parameterName)
            {
                if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
                
                _leftProperties.TryGetValue(parameterName, out var leftProperty);
                var leftValue = leftProperty?.GetValue(_leftInstance);
                
                _rightProperties.TryGetValue(parameterName, out var rightProperty);
                var rightValue = rightProperty?.GetValue(_rightInstance);

                var value = rightValue ?? leftValue;

                return value;
            }
        }
        
        public static TDestination Merge<TDestination, TLeft, TRight>(TLeft left, TRight right)
            where TDestination : class
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            var constructor = GetSuitableConstructor<TDestination>() ?? throw new InvalidOperationException($"Could not find any constructor for type {typeof(TDestination)}.");
            
            return MergeByConstructor<TDestination, TLeft, TRight>(left, right, constructor);
        }

        private static ConstructorInfo GetSuitableConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault();
        }
        
        private static TDestination MergeByConstructor<TDestination, TLeft, TRight>(
            TLeft left,
            TRight right,
            ConstructorInfo constructorInfo)
            where TDestination : class
        {
            var valueResolver = new ValueResolver<TLeft, TRight>(left, right);

            var alreadySetProperties = new List<string>();
            var constructorParameters = new List<object>();

            // 1. Create new instance
            foreach (var parameter in constructorInfo.GetParameters())
            {
                var value = valueResolver.Resolve(parameter.Name);
                
                constructorParameters.Add(value);
                alreadySetProperties.Add(parameter.Name);
            }

            var constructedInstance = constructorInfo.Invoke(constructorParameters.ToArray()) as TDestination
                                      ?? throw new InvalidOperationException(
                                          $"Cannot construct instance of type {typeof(TDestination)}");
            
            // 2. Set any properties on the destination that was not set
            foreach (var pair in GetPropertyDictionary<TDestination>())
            {
                if (alreadySetProperties.Contains(pair.Key)) continue;
                
                var value = valueResolver.Resolve(pair.Key);
                
                pair.Value?.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }

        private static IReadOnlyDictionary<string, PropertyInfo> GetPropertyDictionary<TInstance>()
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
    }
}