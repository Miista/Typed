using System;
using System.Linq.Expressions;

namespace Typesafe.With.Sequence
{
  public interface IWithSequenceBuilder<T>
  {
    IWithSequenceBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty propertyValue);
    IWithSequence<T> ToSequence();
  }
}