using System;
using System.Linq;
using System.Reflection;

namespace Typesafe.With.Lazy
{
  internal static class PropertyValueResolver
  {
    private const string InvokeName = nameof(ValueFactory<object>.Invoke);

    public static object Resolve(object propertyValue)
    {
      // It is OK for the propertyValue to be null. It just means that the code wants to set the property to null.
      if (propertyValue == null) return null;

      var propertyValueType = propertyValue.GetType();
      
      if (propertyValueType.IsGenericType)
      {
        var genericTypeDefinition = propertyValueType.GetGenericTypeDefinition();

        if (genericTypeDefinition.IsAssignableFrom(typeof(ValueFactory<>)))
        {
          var valueFromFactory = ExecuteValueFactory(genericTypeDefinition, propertyValue);
          
          return valueFromFactory;
        }
      }

      return propertyValue;
    }

    private static object ExecuteValueFactory(Type propertyValueType, object propertyValueFactory)
    {
      var genericTypeDefinition = propertyValueType.GetGenericTypeDefinition();
      var genericArgument = propertyValueFactory.GetType().GetGenericArguments().FirstOrDefault()
                            ?? throw new InvalidOperationException(
                              $"Instance of {typeof(ValueFactory<>).Name} lacks a generic type argument.");
      
      var valueFromFactory = genericTypeDefinition.MakeGenericType(genericArgument)
        .InvokeMember(
          name: InvokeName,
          invokeAttr: BindingFlags.InvokeMethod,
          binder: null,
          target: propertyValueFactory,
          args: Array.Empty<object>());

      return valueFromFactory;
    }
  }
}