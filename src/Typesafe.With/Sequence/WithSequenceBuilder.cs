using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Typesafe.Kernel;

namespace Typesafe.With.Sequence
{
  public static class WithSequence
  {
    public static IWithSequenceBuilder<T> New<T>() => new WithSequenceBuilder<T>();

    private class WithSequenceBuilder<T> : IWithSequenceBuilder<T>
    {
      private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

      public IWithSequenceBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue)
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

      public IWithSequence<T> ToSequence() => new WithSequence<T>(_properties);
    }
  }
}