using System.Linq.Expressions;
using System.Reflection;

namespace Typesafe.With
{
    internal class ValueResolver<TInstance>
    {
        private readonly TInstance _instance;

        public ValueResolver(TInstance instance)
        {
            _instance = instance;
        }

        public object Resolve(object value, PropertyInfo existingProperty)
        {
            if (value is LambdaExpression expr)
            {
                var compiledExpression = expr.Compile();
                var existingValue = existingProperty.GetValue(_instance);
                
                return compiledExpression.DynamicInvoke(existingValue);
            }

            return value;
        }
    }
}