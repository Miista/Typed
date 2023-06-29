using System.Linq;
using System.Reflection;

namespace Typesafe.Snapshots.Cloner.Collections
{
    internal class ArrayCloner<TArray> : ITypeCloner<TArray>
    {
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private static TElement[] Clone<TElement>(TElement[] instance)
        {
            var newArray = new TElement[instance.Length];

            for (var i = 0; i < instance.Length; i++)
            {
                var v = instance[i];
                var snapshot = v.GetSnapshot();
                newArray[i] = snapshot;
            }

            return newArray;
        }
        
        public TArray Clone(TArray instance)
        {
            var elementType = typeof(TArray).GetElementType();
            var cloneMethod = GetCloneMethod().MakeGenericMethod(elementType);
            return (TArray) cloneMethod.Invoke(null, new object[] { instance });
        }

        private MethodInfo GetCloneMethod() =>
            GetType()
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == "Clone");
    }
}