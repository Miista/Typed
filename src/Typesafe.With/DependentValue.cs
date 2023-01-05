using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Typesafe.With
{
    internal readonly struct DependentValue
    {
        private readonly LambdaExpression _expression;
        
        public DependentValue(LambdaExpression function)
        {
            _expression = function ?? throw new ArgumentNullException(nameof(function));
        }

        public object Resolve(PropertyInfo existingProperty, object instance)
        {
            if (existingProperty == null) throw new ArgumentNullException(nameof(existingProperty));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            
            var compiledExpression = _expression.Compile();
            var existingValue = existingProperty.GetValue(instance);
                
            return compiledExpression.DynamicInvoke(existingValue);
        }
    }
}