using System;
using Typesafe.Kernel;
using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public class CloneableClonerProvider : ITypeClonerProvider
    {
        public ITypeCloner<T> Resolve<T>()
        {
            if (typeof(T).ImplementsInterface<ICloneable>()) return new CloneableCloner<T>();

            throw new InvalidOperationException($"Type {typeof(T)} does not implement ICloneable");
        }
    }
}