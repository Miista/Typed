using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With.Mutable
{
    public static class ObjectExtensions
    {
        public static T With<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propertyPicker,
            Expression<Func<TProperty, TProperty>> propertyValueFactory
        )
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();

            RequirePropertySetter<T>(propertyName);
            SetProperty(instance, propertyName, new DependentValue(propertyValueFactory));

            return instance;
        }

        public static T With<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propertyPicker,
            TProperty propertyValue)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();

            RequirePropertySetter<T>(propertyName);
            SetProperty(instance, propertyName, propertyValue);

            return instance;
        }

        private static void SetProperty<T>(T instance, string propertyName, object propertyValue)
        {
            var properties = new Dictionary<string, object>{{propertyName, propertyValue}};

            var propertySetter = new PropertySetter<T>();
            propertySetter.SetProperties(instance, properties);
        }

        private static void RequirePropertySetter<T>(string propertyName)
        {
            if (HasPropertySetter<T>(propertyName)) return;
            
            throw new InvalidOperationException($"Property '{propertyName.ToPropertyCase()}' cannot be set via property setter.");
        }
        
        private static bool HasPropertySetter<T>(string propertyName)
        {
            return TypeUtils.GetPropertyDictionary<T>().TryGetValue(propertyName, out var propertyInfo) && propertyInfo.CanWrite;
        }
    }
}