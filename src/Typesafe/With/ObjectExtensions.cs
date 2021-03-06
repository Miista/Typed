﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With
{
    public static class ObjectExtensions
    {
        public static T With<T, TProperty>(this T instance, Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            var properties = new Dictionary<string, object>
            {
                {propertyName, propertyValue}
            };

            var constructor = TypeUtils.GetSuitableConstructor<T>();
            var builder = new UnifiedWithBuilder<T>(constructor);
            
            return builder.Construct(instance, properties);
        }
    }
}