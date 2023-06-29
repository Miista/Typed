namespace Typesafe.Snapshots.Cloner
{
    public interface ITypeCloner { }

    public interface ITypeCloner<T> : ITypeCloner {
        T Clone(T instance);
    }
}