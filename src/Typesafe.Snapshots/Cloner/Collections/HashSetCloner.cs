using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class HashSetCloner<T> : ITypeCloner<HashSet<T>>
    {
        public HashSet<T> Clone(HashSet<T> instance)
        {
            var newList = new HashSet<T>();

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newList.Add(snapshot);
            }

            return newList;
        }
    }
}