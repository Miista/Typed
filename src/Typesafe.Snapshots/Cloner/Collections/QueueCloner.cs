using System.Collections.Generic;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class QueueCloner<T> : ITypeCloner<Queue<T>>
    {
        public Queue<T> Clone(Queue<T> instance)
        {
            var newInstance = new Queue<T>(instance.Count);

            foreach (var v in instance)
            {
                var snapshot = v.GetSnapshot();
                newInstance.Enqueue(snapshot);
            }

            return newInstance;
        }
    }
}