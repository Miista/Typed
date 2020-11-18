using System;
using System.Linq;
using System.Reflection;
using Typesafe.Builders;

namespace Typesafe.Factories
{
    internal static class WithBuilderFactory
    {
        public static IWithBuilder<T> Create<T>()
        {
            var constructor = GetSuitableConstructor<T>() ?? throw new InvalidOperationException($"Could not find any constructor for type {typeof(T)}.");
            
            return new MixedConstructorAndPropertyWithBuilder<T>(constructor);
        }

        private static ConstructorInfo GetSuitableConstructor<T>()
        {
            return typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault();
        }
    }
}