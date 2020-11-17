using System;
using System.Collections.Generic;
using Typesafe.Utils;

namespace Typesafe.Builders
{
    internal class PropertyWithBuilder<T> : IWithBuilder<T>
    {
        public T Construct(T instance, IReadOnlyDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            
            return WithByProperty(instance, properties);
        }

        private static TInstance WithByProperty<TInstance>(
            TInstance instance, 
            IReadOnlyDictionary<string, object> properties)
        {
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>();

            var constructedInstance = Activator.CreateInstance<TInstance>();
            
            foreach (var parameter in existingProperties)
            {
                if (!parameter.Value.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{parameter.Key}' cannot be written to.");
                }
                
                existingProperties.TryGetValue(parameter.Key, out var existingProperty);
                var originalValue = existingProperty?.GetValue(instance);
                var hasNewValue = properties.TryGetValue(parameter.Key, out var newValue);

                var value = hasNewValue ? newValue : originalValue;

                parameter.Value.SetValue(constructedInstance, value);
            }

            return constructedInstance;
        }
    }
}