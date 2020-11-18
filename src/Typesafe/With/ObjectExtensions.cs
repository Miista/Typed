using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Typesafe.Factories;
using Typesafe.Utils;

namespace Typesafe.With
{
    public static class ObjectExtensions
    {
        public static T With<T, TProperty>(this T instance, Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetProperty();
            var properties = new Dictionary<string, object>
            {
                {propertyName.Name, propertyValue}
            };

            return WithBuilderFactory.Create<T>().Construct(instance, properties);
        }
    }
}