using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class DictionaryCloner<TKey, TValue> : ITypeCloner<Dictionary<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> Clone(Dictionary<TKey, TValue> instance)
        {
            var newInstance = new Dictionary<TKey, TValue>(instance.Count);

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newInstance.Add(snapshot.Key, snapshot.Value);
            }

            return newInstance;
        }
    }
}