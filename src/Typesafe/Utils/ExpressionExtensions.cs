﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Typesafe.Utils
{
    internal static class ExpressionExtensions
    {
        public static string GetProperty<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            
            switch (expression.Body)
            {
                case UnaryExpression u when u.Operand is MemberExpression um:
                    return (um.Member as PropertyInfo)?.Name.ToPropertyCase();
                case MemberExpression m:
                    return (m.Member as PropertyInfo)?.Name.ToPropertyCase();
                default:
                    throw new InvalidOperationException($"Cannot retrieve property from expression '{expression}'");
            }
        }
    }
}