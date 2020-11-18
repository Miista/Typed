using System;
using Typesafe.Builders;
using Typesafe.Utils;

namespace Typesafe.Factories
{
    internal static class WithBuilderFactory
    {
        public static IWithBuilder<T> Create<T>()
        {
            var constructor = TypeUtils.GetSuitableConstructor<T>() ?? throw new InvalidOperationException($"Could not find any constructor for type {typeof(T)}.");
            
            return new UnifiedWithBuilder<T>(constructor);
        }
    }
}