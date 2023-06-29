using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public interface ITypeClonerProvider
    {
        ITypeCloner<T> Resolve<T>();
    }
}