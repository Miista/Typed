using System;

namespace Typesafe.Snapshots.Cloner
{
    public class CloneableCloner<T> : ITypeCloner<T>
    {
        public T Clone(T instance) => (T) ((ICloneable) instance).Clone();
    }
}