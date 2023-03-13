using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class SortedSetCloner<T> : ITypeCloner<SortedSet<T>>
    {
        public SortedSet<T> Clone(SortedSet<T> instance)
        {
            var newList = new SortedSet<T>();

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newList.Add(snapshot);
            }

            return newList;
        }
    }
}