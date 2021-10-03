using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Typesafe.Kernel;
using Typesafe.With.Sequence;

namespace Typesafe.With.Lazy
{
  public class LazyInstancedWithSequence<T> where T : class
  {
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
    private readonly T _instance;
    
    public LazyInstancedWithSequence(T instance)
    {
      _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public LazyInstancedWithSequence<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
    {
      if (propertyPicker == null) throw new ArgumentNullException(nameof(propertyPicker));
      
      var propertyName = propertyPicker.GetPropertyName();
      AddOrUpdate(propertyName, propertyValue);

      return this;
    }
    
    private void AddOrUpdate<TProperty>(string propertyName, TProperty propertyValue)
    {
      if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

      if (_properties.ContainsKey(propertyName))
      {
        _properties[propertyName] = propertyValue;
      }
      else
      {
        _properties.Add(propertyName, propertyValue);
      }
    }

    private T Apply()
    {
      var sequence = new WithSequence<T>(_properties);
      var appliedInstance = sequence.ApplyTo(_instance);

      return appliedInstance;
    }

    public static implicit operator T(LazyInstancedWithSequence<T> builder) =>
      builder.Apply();
  }
}