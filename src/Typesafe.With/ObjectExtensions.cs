using System;
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

            PropertyValidator.Validate<T>(propertyName);

            var valueResolver = new WithValueResolver<T>(instance, propertyName, propertyValue);
            var instanceBuilder = new InstanceBuilder<T>(valueResolver);
            return instanceBuilder.Construct();
        }
    }
}