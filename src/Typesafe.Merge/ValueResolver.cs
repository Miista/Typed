using System;
using System.Collections.Generic;
using System.Reflection;
using Typesafe.Kernel;

namespace Typesafe.Merge
{
    internal class ValueResolver<TDestination, TLeft, TRight>
    {
        private readonly TLeft _leftInstance;
        private readonly TRight _rightInstance;
        private readonly IReadOnlyDictionary<string, PropertyInfo> _leftProperties;
        private readonly IReadOnlyDictionary<string, PropertyInfo> _rightProperties;

        public ValueResolver(TLeft leftInstance, TRight rightInstance)
        {
            _leftInstance = leftInstance;
            _rightInstance = rightInstance;
            _leftProperties = TypeUtils.GetPropertyDictionary<TLeft>();
            _rightProperties = TypeUtils.GetPropertyDictionary<TRight>();
        }

        public object Resolve(ParameterInfo parameterInfo)
        {
            if (parameterInfo == null) throw new ArgumentNullException(nameof(parameterInfo));

            return Resolve(parameterInfo.Name);
        }
        
        public object Resolve(string parameterName)
        {
            if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
                
            _leftProperties.TryGetValue(parameterName, out var leftProperty);
            var leftValue = leftProperty?.GetValue(_leftInstance);
                
            _rightProperties.TryGetValue(parameterName, out var rightProperty);
            var rightValue = rightProperty?.GetValue(_rightInstance);

            var value = rightValue ?? leftValue;

            return value;
        }
    }
}