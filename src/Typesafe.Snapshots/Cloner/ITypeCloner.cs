namespace Typesafe.Snapshots.Cloner
{
    public interface ITypeCloner { }

    public interface ITypeClonerProvider : ITypeCloner
    {
        ITypeCloner<T> Resolve<T>();
    }

    public interface ITypeCloner<T> : ITypeCloner {
        T Clone(T instance);
    }
}