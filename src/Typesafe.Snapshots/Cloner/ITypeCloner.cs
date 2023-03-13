namespace Typesafe.Snapshots.Cloner
{
    internal interface ITypeCloner<T>
    {
        T Clone(T instance);
    }
}