using System;
using System.Linq;

namespace TypeWither
{
    internal static class TypeWither
    {
        internal static WithBuilder<T> GetBuilder<T>(T instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            
            return new WithBuilder<T>(instance);
        }

        internal static T With<T>(T instance, params PropertyValue[] properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var propertyDictionary = properties.ToDictionary(tuple => tuple.PropertyName.ToLowerInvariant(), tuple => tuple.Value);
            var withBuilder = WithBuilderFactory.Create<T>(propertyDictionary);
            
            return withBuilder.Construct(instance, propertyDictionary);
        }
    }
}