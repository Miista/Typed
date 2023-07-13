using System;
using System.Collections.Generic;

namespace Typesafe.With.Lazy
{
    internal static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> self,
            TKey key,
            TValue value
        )
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            if (self.ContainsKey(key))
            {
                self[key] = value;
            }
            else
            {
                self.Add(key, value);
            }

            return self;
        }
    }
}