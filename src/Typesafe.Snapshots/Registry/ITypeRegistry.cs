using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public interface ITypeRegistry
    {
        ITypeCloner<T> TryGetCloner<T>();
    }
}