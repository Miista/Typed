namespace Typesafe.Snapshots.Cloner
{
    internal class PrimitiveCloner<T> : ITypeCloner<T>
    {
        public T Clone(T instance)
        {
            return (T) instance;
        }
    }
}