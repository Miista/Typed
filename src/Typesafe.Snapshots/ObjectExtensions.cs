using Typesafe.Snapshots.Registry;

namespace Typesafe.Snapshots
{
    public static class ObjectExtensions
    {
        public static T GetSnapshot<T>(this T self)
        {
            if (object.ReferenceEquals(null, self)) return default(T);

            var typeRegistry = new TypeRegistryBuilder().Build();
            var cloner = typeRegistry.TryGetCloner<T>();
            var clone = cloner.Clone(self);

            return clone;
        }
    }
}