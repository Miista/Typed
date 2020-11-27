using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Typesafe.With")]
[assembly: InternalsVisibleTo("Typesafe.Merge")]
[assembly: InternalsVisibleTo("Typesafe")]
namespace Typesafe.Kernel
{
    internal static class ExpressionExtensions
    {
        public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            
            switch (expression.Body)
            {
                case UnaryExpression u when u.Operand is MemberExpression um:
                    return (um.Member as PropertyInfo)?.Name.ToParameterCase();
                case MemberExpression m:
                    return (m.Member as PropertyInfo)?.Name.ToParameterCase();
                default:
                    throw new InvalidOperationException($"Cannot retrieve property from expression '{expression}'");
            }
        }
    }
}