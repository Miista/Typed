namespace Typesafe.Kernel
{
    public interface IValueResolver<TInstance>
    {
        object Resolve(string parameterName);
    }
}