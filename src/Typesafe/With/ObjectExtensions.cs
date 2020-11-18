using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Typesafe.Factories;

namespace Typesafe.With
{
    public static class ObjectExtensions
    {
        public static T With<T, TProperty>(this T instance, Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = GetPropertyName(propertyPicker);
            var properties = new Dictionary<string, object>
            {
                {propertyName.Name, propertyValue}
            };

            return WithBuilderFactory.Create<T>().Construct(instance, properties);
        }
        
        private static PropertyInfo GetPropertyName<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            switch (expression.Body)
            {
                case UnaryExpression u when u.Operand is MemberExpression um:
                    return um.Member as PropertyInfo;
                case MemberExpression m:
                    return m.Member as PropertyInfo;
                default:
                    throw new InvalidOperationException($"Cannot retrieve property from expression '{expression}'");
            }
        }
    }
}