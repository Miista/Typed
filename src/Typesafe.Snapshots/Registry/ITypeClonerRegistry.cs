using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public interface ITypeClonerRegistry
    {
        bool TryGetCloner<T>(out ITypeCloner<T> cloner);
    }
}