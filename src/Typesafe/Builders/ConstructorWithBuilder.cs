using System;
using System.Collections.Generic;
using System.Reflection;
using Typesafe.Utils;

namespace Typesafe.Builders
{
    internal class ConstructorWithBuilder<T> : IWithBuilder<T>
    {
        private readonly ConstructorInfo _constructorInfo;

        public ConstructorWithBuilder(ConstructorInfo constructorInfo)
        {
            _constructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        }

        public T Construct(T instance, IReadOnlyDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            
            return WithByConstructor(instance, properties, _constructorInfo);
        }

        private static string UppercaseFirstLetter(string s)
        {
            var firstLetter = s[0];
            var firstLetterInUppercase = char.ToUpperInvariant(firstLetter);
            var remainingString = s.Substring(1);
            
            return firstLetterInUppercase + remainingString;
        }
        
        private static TInstance WithByConstructor<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties,
            ConstructorInfo constructorInfo)
        {
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();

            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                existingProperties.TryGetValue(UppercaseFirstLetter(parameter.Name), out var existingProperty);
                var originalValue = existingProperty?.GetValue(instance);
                
                var hasNewValue = newProperties.TryGetValue(UppercaseFirstLetter(parameter.Name), out var newValue);

                var value = hasNewValue ? newValue : originalValue;
                
                parameters.Add(value);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) is TInstance
                ? (TInstance) constructorInfo.Invoke(parameters.ToArray())
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");

            return constructedInstance;
        }
    }
}