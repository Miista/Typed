using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class ListCloner<T> : ITypeCloner<List<T>>
    {
        public List<T> Clone(List<T> instance)
        {
            var newList = new List<T>(instance.Count);

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newList.Add(snapshot);
            }

            return newList;
        }
    }
}