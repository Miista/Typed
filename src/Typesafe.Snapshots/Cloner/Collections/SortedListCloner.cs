using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class SortedListCloner<TKey, TValue> : ITypeCloner<SortedList<TKey, TValue>>
    {
        public SortedList<TKey, TValue> Clone(SortedList<TKey, TValue> instance)
        {
            var newList = new SortedList<TKey, TValue>(instance.Count);

            foreach (var v in instance)
            {
                var snapshotKey = v.Key.GetSnapshot();
                var snapshotValue = v.Value.GetSnapshot();
                newList.Add(snapshotKey, snapshotValue);
            }

            return newList;
        }
    }
}