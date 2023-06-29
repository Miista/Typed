using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class LinkedListCloner<T> : ITypeCloner<LinkedList<T>>
    {
        public LinkedList<T> Clone(LinkedList<T> instance)
        {
            var newList = new LinkedList<T>();

            var lastNode = newList.First;
            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();

                if (lastNode == null)
                {
                    lastNode = newList.AddFirst(snapshot);
                }
                else
                {
                    lastNode = newList.AddAfter(lastNode, snapshot);
                }
            }

            return newList;
        }
    }
}