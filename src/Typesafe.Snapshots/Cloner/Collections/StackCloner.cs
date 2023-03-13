using System.Collections.Generic;
using System.Linq;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class StackCloner<T> : ITypeCloner<Stack<T>>
    {
        public Stack<T> Clone(Stack<T> instance)
        {
            var newInstance = new Stack<T>(instance.Count);

            foreach (var v in instance.Reverse())
            {
                var snapshot = v.GetSnapshot();
                newInstance.Push(snapshot);
            }

            return newInstance;
        }
    }
}