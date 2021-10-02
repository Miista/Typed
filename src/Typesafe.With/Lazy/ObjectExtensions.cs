using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With.Lazy
{
    public class LazyWithSequence<T>
    {
        public static LazyWithSequenceBuilder<T> New() => new LazyWithSequenceBuilder<T>(new Dictionary<string, object>());
        
        protected readonly Dictionary<string, object> Properties;

        internal LazyWithSequence(Dictionary<string, object> properties)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }
        
        protected void AddOrUpdate<TProperty>(string propertyName, TProperty propertyValue)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            if (Properties.ContainsKey(propertyName))
            {
                Properties[propertyName] = propertyValue;
            }
            else
            {
                Properties.Add(propertyName, propertyValue);
            }
        }
        
        public T ApplyTo(T instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            
            var constructor = TypeUtils.GetSuitableConstructor<T>();
            var builder = new UnifiedWithBuilder<T>(constructor);
            
            return builder.Construct(instance, Properties);
        }
    }
    
    public class LazyWithSequenceBuilder<T>
    {
        public static LazyWithSequenceBuilder<T> New() => new LazyWithSequenceBuilder<T>(new Dictionary<string, object>());

        private readonly Dictionary<string, object> Properties;

        internal LazyWithSequenceBuilder(Dictionary<string, object> properties)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }
        
        public LazyWithSequenceBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
        {
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            AddOrUpdate(propertyName, propertyValue);

            return this;
        }

        private void AddOrUpdate<TProperty>(string propertyName, TProperty propertyValue)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            if (Properties.ContainsKey(propertyName))
            {
                Properties[propertyName] = propertyValue;
            }
            else
            {
                Properties.Add(propertyName, propertyValue);
            }
        }
        
        public LazyWithSequence<T> ToSequence() => new LazyWithSequence<T>(Properties);
    }

    
    public class InstancedWithSequence<T> : LazyWithSequence<T> where T : class
    {
        private readonly T _instance;

        internal InstancedWithSequence(T instance) : base(new Dictionary<string, object>())
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public LazyWithSequence<T> ToSequence() => new LazyWithSequence<T>(Properties);
        
        public static implicit operator T(InstancedWithSequence<T> builder) => builder.ApplyTo(builder._instance);

        public new InstancedWithSequence<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
        {
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            AddOrUpdate(propertyName, propertyValue);

            return this;
        }
    }
    
    public static class ObjectExtensions
    {
        public static InstancedWithSequence<T> With<T, TProperty>(this T instance, Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));

            var propertyName = propertyPicker.GetPropertyName();
            Validate<T>(propertyName);

            return new InstancedWithSequence<T>(instance).With(propertyPicker, propertyValue);
        }

        private static void Validate<T>(string propertyName)
        {
            // Can we set the property via constructor?
            var hasConstructorParameter = HasConstructorParameter<T>(propertyName);
            
            if (hasConstructorParameter) return;
            
            // Can we set the property via property setter?
            var hasPropertySetter = HasPropertySetter<T>(propertyName);
            
            if (hasPropertySetter) return;

            // If we cannot do either, then there is no point in continuing.
            throw new InvalidOperationException($"Property '{propertyName.ToPropertyCase()}' cannot be set via constructor or property setter.");
        }

        private static bool HasPropertySetter<T>(string propertyName)
        {
            return TypeUtils.GetPropertyDictionary<T>().TryGetValue(propertyName, out var propertyInfo) && propertyInfo.CanWrite;
        }

        private static bool HasConstructorParameter<T>(string propertyName)
        {
            var constructorParameters = TypeUtils.GetSuitableConstructor<T>().GetParameters();
            
            // Can we find a matching constructor parameter?
            var hasConstructorParameter = constructorParameters
                .Any(info => string.Equals(info.Name, propertyName, StringComparison.Ordinal));
            
            if (hasConstructorParameter) return true;

            // Can we find a matching constructor parameter if we lowercase both parameter and property name?
            var hasConstructorParameterByLowercase = constructorParameters
                .Any(info => string.Equals(info.Name, propertyName, StringComparison.InvariantCultureIgnoreCase));

            if (hasConstructorParameterByLowercase) return true;

            return false;
        }
    }
}