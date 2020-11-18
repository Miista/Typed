using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Utils
{
    internal static class TypeUtils
    {
        public static Dictionary<string, PropertyInfo> GetPropertyDictionary<T>() => 
            typeof(T)
                .GetProperties()
                .ToDictionary(info => info.Name)
                .Select(LowercaseKey)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            
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