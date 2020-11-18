using System;
using Typesafe.Builders;
using Typesafe.Utils;

namespace Typesafe.Factories
{
    internal static class WithBuilderFactory
    {
        public static IWithBuilder<T> Create<T>()
        {
            var constructor = TypeUtils.GetSuitableConstructor<T>();
            
            return new UnifiedWithBuilder<T>(constructor);
        }
    }
}