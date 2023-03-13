using System;
using Typesafe.Snapshots.Cloner;

namespace Typesafe.Snapshots.Registry
{
    public interface ITypeRegistryBuilder
    {
        void RegisterProvider(Type type, ITypeClonerProvider provider);
        void RegisterCloner<T>(Type type, ITypeCloner<T> cloner);
    }
}