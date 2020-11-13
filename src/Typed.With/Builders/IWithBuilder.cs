using System.Collections.Generic;

namespace TypeWither.Builders
{
    internal interface IWithBuilder<T>
    {
        T Construct(T instance, IReadOnlyDictionary<string, object> properties);
    }
}