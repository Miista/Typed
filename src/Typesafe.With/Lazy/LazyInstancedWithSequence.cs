using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Typesafe.Kernel;

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

    public static implicit operator T(LazyInstancedWithSequence<T> builder) =>
      new Sequence.WithSequence<T>(builder._properties).ApplyTo(builder._instance);
  }
}