using System;
using System.Collections.Generic;
using System.Linq;
using Typesafe.Utils;

namespace Typesafe.Builders
{
    internal class PropertySetter
    {
        /// <summary>
        /// Mutates <paramref name="instance"/>, setting all properties in <paramref name="newProperties"/>.
        /// </summary>
        /// <param name="instance">The instance to mutate.</param>
        /// <param name="newProperties">The properties to set.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>A mutated instance.</returns>
        /// <exception cref="InvalidOperationException">If the property does not exist or cannot be written to.</exception>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        public static TInstance EnrichByProperty<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (newProperties == null) throw new ArgumentNullException(nameof(newProperties));
            
            var existingProperties = TypeUtils.GetPropertyDictionary<TInstance>().Select(LowercaseKey).ToDictionary(pair => pair.Key, pair => pair.Value);;

            foreach (var property in newProperties)
            {
                if (!existingProperties.TryGetValue(property.Key, out var existingProperty))
                {
                    throw new InvalidOperationException($"Cannot find property with name '{property.Key}'.");
                }

                if (!existingProperty.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{property.Key}' cannot be written to.");
                }
                
                existingProperty.SetValue(instance, property.Value);
            }

            return instance;
        }
        
        private static KeyValuePair<string, TValue> LowercaseKey<TValue>(KeyValuePair<string, TValue> pair)
        {
            var lowercasedKey = Lowercase(pair.Key);

            return new KeyValuePair<string, TValue>(lowercasedKey, pair.Value);

            string Lowercase(string s)
            {
                var firstLetter = s[0];
                var firstLetterInUppercase = char.ToLowerInvariant(firstLetter);
                var remainingString = s.Substring(1);
                    
                return firstLetterInUppercase + remainingString;
            }
        }
    }
}