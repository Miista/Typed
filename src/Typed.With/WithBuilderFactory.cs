using System;
using System.Collections.Generic;
using Type.Core;
using TypeWither.Builders;

namespace TypeWither
{
    internal static class WithBuilderFactory
    {
        public static IWithBuilder<T> Create<T>(Dictionary<string, object> properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            
            var constructorInfo = ReflectionUtils.GetSuitableConstructor<T>();
            
            if (constructorInfo == null) return new PropertyWithBuilder<T>();
            if (constructorInfo.CanSatisfy(properties)) return new ConstructorWithBuilder<T>(constructorInfo);
            return new MixedConstructorAndPropertyWithBuilder<T>(constructorInfo);
        }
    }
}