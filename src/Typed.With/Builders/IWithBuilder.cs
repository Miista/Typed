using System.Collections.Generic;

namespace Typed.With.Builders
{
    internal interface IWithBuilder<T>
    {
        T Construct(T instance, IReadOnlyDictionary<string, object> properties);
    }
}