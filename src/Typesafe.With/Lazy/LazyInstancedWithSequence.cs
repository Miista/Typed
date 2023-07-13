using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Typesafe.Kernel;
using Typesafe.With.Sequence;

namespace Typesafe.With.Lazy
{
  public class LazyInstancedWithSequence<T> where T : class
  {
    private readonly Dictionary<string, object> _properties;
    private readonly T _instance;

    public LazyInstancedWithSequence(T instance)
      : this(instance, new Dictionary<string, object>())
    {
    }

    private LazyInstancedWithSequence(T instance, Dictionary<string, object> properties)
    {
      _instance = instance ?? throw new ArgumentNullException(nameof(instance));
      _properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    public LazyInstancedWithSequence<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
    {
      if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));
      
      var propertyName = propertyPicker.GetPropertyName();
      
      var dictionary = new Dictionary<string, object>(_properties).AddOrUpdate(propertyName, propertyValue);
      
      return new LazyInstancedWithSequence<T>(_instance, dictionary);
    }
    
    public LazyInstancedWithSequence<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<TProperty> propertyValueFactory)
    {
      if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));
      if (propertyValueFactory == null) throw new ArgumentNullException(nameof(propertyValueFactory));

      var propertyName = propertyPicker.GetPropertyName();
      var valueFactory = new ValueFactory<TProperty>(propertyValueFactory);
      var dictionary = new Dictionary<string, object>(_properties).AddOrUpdate(propertyName, valueFactory);

      return new LazyInstancedWithSequence<T>(_instance, dictionary);
    }

    private T Apply()
    {
      var resolvedProperties = ResolveLazyPropertyValues(_properties);
      var sequence = new WithSequence<T>(resolvedProperties);
      var appliedInstance = sequence.ApplyTo(_instance);

      return appliedInstance;
    }

    private static Dictionary<string, object> ResolveLazyPropertyValues(Dictionary<string, object> dictionary)
    {
      return dictionary
        .Select(pair => new KeyValuePair<string, object>(pair.Key, PropertyValueResolver.Resolve(pair.Value)))
        .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static implicit operator T(LazyInstancedWithSequence<T> builder) => builder.Apply();
  }
}