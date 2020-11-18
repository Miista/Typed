using System;
using System.Collections.Generic;
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

            var constructor = TypeUtils.GetSuitableConstructor<TDestination>() ?? throw new InvalidOperationException($"Could not find any constructor for type {typeof(TDestination)}.");
            
            return MergeByConstructor<TDestination, TLeft, TRight>(left, right, constructor);
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
            foreach (var pair in (IReadOnlyDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TDestination>())
            {
                if (alreadySetProperties.Contains(pair.Key)) continue;
                
                var value = valueResolver.Resolve(pair.Key);
                
                pair.Value?.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }
    }
}