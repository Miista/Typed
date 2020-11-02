using System;
using System.Linq.Expressions;

namespace TypeWither
{
    public static class ObjectExtensions
    {
        public static WithBuilder<T> With<T, TValue>(this T entity, Expression<Func<T, TValue>> selector, TValue value)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return TypeWither.GetBuilder(entity).With(selector, value);
        }
    }
}