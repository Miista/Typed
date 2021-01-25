using System;
using System.Collections.Generic;
using System.Linq;
using Typesafe.Kernel;

namespace Typesafe.With
{
    internal class WithValueResolver<T> : IValueResolver<T>
    {
        private readonly IReadOnlyDictionary<string, object> _values;

        public WithValueResolver(T instance, string propertyName, object propertyValue)
        {
            _values = typeof(T)
                ?.GetProperties()
                ?.Select(info =>
                {
                    var value = string.Equals(propertyName, StringExtensions.ToParameterCase(info.Name)) ? propertyValue : info.GetValue(instance);
                    
                    return new KeyValuePair<string, object>(StringExtensions.ToParameterCase(info.Name), value);
                })
                ?.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public object Resolve(string parameterName)
        {
            if (_values.TryGetValue(parameterName, out var valueByExactMatch))
            {
                return valueByExactMatch;
            }

            if (_values.TryGetValue(parameterName.ToParameterCase(), out var valueByParameterCase))
            {
                return valueByParameterCase;
            }

            if (_values.TryGetValue(parameterName.ToLowerInvariant(), out var valueByLowercased))
            {
                return valueByLowercased;
            }

            throw new InvalidOperationException($"Property '{parameterName.ToPropertyCase()}' cannot be set via constructor or property setter.");
        }
    }
}