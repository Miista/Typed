namespace Typesafe.Snapshots.Registry
{
    public interface ITypeClonerRegistrar
    {
        // ReSharper disable once UnusedParameter.Global
        void RegisterTypeCloners(ITypeRegistryBuilder typeRegistryBuilder);
    }
}