using System;
using System.Linq;
using System.Reflection;
using Typesafe.Kernel;

namespace Typesafe.Merge
{
    internal class UnifiedMergeBuilder
    {
        public TDestination Construct<TDestination, TLeft, TRight>(
            TLeft left,
            TRight right)
            where TDestination : class
        {
            var constructorInfo = TypeUtils.GetSuitableConstructor<TDestination>();
            var valueResolver = new MergeValueResolver<TDestination, TLeft, TRight>(left, right);

            // 1. Create new instance
            var constructedInstance = ConstructInstance<TDestination, TLeft, TRight>(constructorInfo, valueResolver);
            
            // 2. Set any properties on the destination that was not set
            var destinationProperties = TypeUtils.GetPropertyDictionary<TDestination>();
            var constructorParameterNames = constructorInfo
                .GetParameters()
                .ToLookup(info => info.Name);
            
            foreach (var pair in destinationProperties)
            {
                if (constructorParameterNames.Contains(pair.Key)) continue;
                
                var value = valueResolver.Resolve(pair.Key);
                pair.Value?.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }

        private static TInstance ConstructInstance<TInstance, TLeft, TRight>(
                ConstructorInfo constructorInfo,
                MergeValueResolver<TInstance, TLeft, TRight> valueResolver) where TInstance : class
        {
            // Resolve parameter values
            var constructorParameters = constructorInfo
                .GetParameters()
                .Select(info => info)
                .Select(valueResolver.Resolve)
                .ToArray();

            // Create instance
            var constructedInstance = constructorInfo.Invoke(constructorParameters) as TInstance
                                      ?? throw new InvalidOperationException(
                                          $"Cannot construct instance of type {typeof(TInstance)}");

            return constructedInstance;
        }
    }
}