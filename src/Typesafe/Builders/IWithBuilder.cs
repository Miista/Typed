using System.Collections.Generic;

namespace Typesafe.Builders
{
    internal interface IWithBuilder<T>
    {
        T Construct(T instance, IReadOnlyDictionary<string, object> properties);
    }
}