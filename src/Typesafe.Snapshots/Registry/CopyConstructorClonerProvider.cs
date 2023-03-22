using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    internal class CopyConstructorClonerProvider : ITypeClonerProvider
    {
        public ITypeCloner<T> Resolve<T>() => new CopyConstructorCloner<T>();
    }
}