using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Typesafe.With
{
    internal readonly struct DependentValue
    {
        private readonly Delegate _expression;
        private readonly object _instance;
        
        public DependentValue(LambdaExpression function, object instance)
        {
            _expression = function?.Compile() ?? throw new ArgumentNullException(nameof(function));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public object Resolve(PropertyInfo existingProperty)
        {
            if (existingProperty == null) throw new ArgumentNullException(nameof(existingProperty));

            var compiledExpression = _expression;
            var existingValue = existingProperty.GetValue(_instance);
                
            return compiledExpression.DynamicInvoke(existingValue);
        }
    }
}