using System;
using System.Collections.Generic;
using Typesafe.Kernel;

namespace Typesafe.With.Sequence
{
  internal class WithSequence<T> : IWithSequence<T>
  {
    private readonly Dictionary<string, object> _properties;

    public WithSequence(Dictionary<string, object> properties)
    {
      _properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    public T ApplyTo(T instance)
    {
      if (instance == null) throw new ArgumentNullException(nameof(instance));

      var constructor = TypeUtils.GetSuitableConstructor<T>();
      var builder = new UnifiedWithBuilder<T>(constructor);

      return builder.Construct(instance, _properties);
    }
  }
}