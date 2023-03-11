using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Snapshots
{
    internal class ValueCloner
    {
        public object Clone(object instance)
        {
            if (instance == null) return null;

            switch (instance)
            {
                case string s:
                    return string.Copy(s);
                case int i:
                    return i;
                case double d:
                    return d;
                case decimal d:
                    return d;
                case byte b:
                    return b;
                case short s:
                    return s;
                case long l:
                    return l;
                case bool b:
                    return b;
                default:
                    break;
            }

            if (instance.GetType().IsValueType)
            {
                var getSnapshotMethod = GetType().GetMethod(nameof(GetSnapshot), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(instance.GetType());
                var clonedInstance = getSnapshotMethod.Invoke(this, new object[]{instance});
                return clonedInstance;
            }
            
            if (instance.GetType().IsGenericType && instance.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                var first = instance.GetType().GenericTypeArguments.First();
                var cloneListMethod = GetType().GetMethod(nameof(CloneList), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(first);
                var clonedList = cloneListMethod.Invoke(this, new object[]{instance, this});
                return clonedList;
            }

            return instance.GetSnapshot();
        }

        private T GetSnapshot<T>(T instance) => instance.GetSnapshot();
        
        private List<T> CloneList<T>(List<T> originalList, ValueCloner valueCloner)
        {
            var newList = new List<T>(originalList.Count);

            foreach (var v in originalList)
            {
                var snapshot = v.GetSnapshot();
                newList.Add(snapshot);
            }

            return newList;
        }
    }
}