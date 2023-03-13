using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class SortedDictionaryCloner<TKey, TValue> : ITypeCloner<SortedDictionary<TKey, TValue>>
    {
        public SortedDictionary<TKey, TValue> Clone(SortedDictionary<TKey, TValue> instance)
        {
            var newInstance = new SortedDictionary<TKey, TValue>();

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newInstance.Add(snapshot.Key, snapshot.Value);
            }

            return newInstance;
        }
    }
}