namespace Typesafe.Snapshots.Cloner
{
    internal class CopyConstructorCloner<T> : ITypeCloner<T>
    {
        public T Clone(T instance)
        {
            var copyConstructor = typeof(T).GetCopyConstructor();
            var copiedInstance = copyConstructor.Invoke(new object[]{instance});

            return (T)copiedInstance;
        }
    }
}