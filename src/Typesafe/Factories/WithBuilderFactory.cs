using System;
using System.Collections.Generic;
using Typesafe.Builders;
using Typesafe.Utils;

namespace Typesafe.Factories
{
    internal static class WithBuilderFactory
    {
        public static IWithBuilder<T> Create<T>(Dictionary<string, object> properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var satisfiedConstructor = ReflectionUtils.GetSatisfiedConstructor<T>(properties);
            if (satisfiedConstructor != null) return new ConstructorWithBuilder<T>(satisfiedConstructor);

            var partiallySatisfiedConstructor = ReflectionUtils.GetPartiallySatisfiedConstructor<T>();
            if (partiallySatisfiedConstructor != null) return new MixedConstructorAndPropertyWithBuilder<T>(partiallySatisfiedConstructor);
            
            return new PropertyWithBuilder<T>();
        }
    }
}