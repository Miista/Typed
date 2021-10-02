namespace Typesafe.With.Sequence
{
  public interface IWithSequence<T>
  {
    T ApplyTo(T instance);
  }
}