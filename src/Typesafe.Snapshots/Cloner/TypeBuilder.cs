using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Kernel;

namespace Typesafe.Snapshots.Cloner
{
    internal class TypeBuilder<T>
    {
        private readonly ConstructorInfo _constructorInfo;

        public TypeBuilder(ConstructorInfo constructorInfo)
        {
            _constructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
        }

        public T Construct(T instance, IReadOnlyDictionary<string, object> properties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            // 1. Construct instance of T (and set properties via constructor)
            var constructedInstance = CreateInstanceViaConstructor(instance, _constructorInfo);
            
            // 2. Copy remaining properties
            var copyProperties = GetCopyProperties(_constructorInfo, properties);
            var constructedInstanceWithCopiedProperties = CopyProperties(instance, constructedInstance, copyProperties);
            
            return constructedInstanceWithCopiedProperties;
        }

        private static TInstance CreateInstanceViaConstructor<TInstance>(
            TInstance instance,
            ConstructorInfo constructorInfo
        )
        {
            var existingProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TInstance>();

            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                var existingProperty = TryFindExistingProperty(parameter);

                if (existingProperty == null)
                    throw new Exception($"Cannot find value for constructor parameter '{parameter.Name}' on type {typeof(TInstance)}");
                
                var originalValue = existingProperty.GetValue(instance);
                var clonedProperty = CloneValue(existingProperty.PropertyType, originalValue);
                
                parameters.Add(clonedProperty);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) is TInstance
                ? (TInstance) constructorInfo.Invoke(parameters.ToArray())
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");

            return constructedInstance;
            
            PropertyInfo TryFindExistingProperty(ParameterInfo parameterInfo)
            {
                // Can we find a matching constructor parameter?
                if (existingProperties.TryGetValue(parameterInfo.Name, out var existingPropertyByExactMatch))
                {
                    return existingPropertyByExactMatch;
                }

                // Can we find a matching constructor parameter if we lowercase both parameter and property name?
                var existingPropertyKey =
                    existingProperties.Keys.FirstOrDefault(key => string.Equals(key, parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    ?? throw new InvalidOperationException($"Cannot find property for constructor parameter '{parameterInfo.Name}'.");

                var existingPropertyByLowercaseMatch = existingProperties[existingPropertyKey];
                
                return existingPropertyByLowercaseMatch;
            }
        }

        private static T CopyProperties(T source, T destination, IEnumerable<PropertyInfo> properties)
        {
            foreach (var kvp in properties)
            {
                var value = kvp.GetValue(source);

                var clone = CloneValue(kvp.PropertyType, value);
                kvp.SetValue(destination, clone);
            }

            return destination;
        }

        private static IEnumerable<PropertyInfo> GetCopyProperties(
            ConstructorInfo constructorInfo,
            IReadOnlyDictionary<string, object> excludeProperties)
        {
            var publicProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<T>();
            
            // Remove properties already set
            foreach (var parameter in excludeProperties.Select(kvp => kvp.Key))
            {
                publicProperties.Remove(parameter);
            }
            
            // Remove properties set via constructor
            foreach (var parameter in GetConstructorParameterNames())
            {
                publicProperties.Remove(parameter);
            }

            return publicProperties.Values.Where(info => info.CanWrite);
            
            IEnumerable<string> GetConstructorParameterNames()
            {
                return constructorInfo
                    .GetParameters()
                    .Select(parameterInfo => parameterInfo.Name)
                    .ToList();
            }
        }

        private static object CloneValue(Type propertyType, object value)
        {
            var clone = ReflectionHelper.MakeGenericMethod<TypeBuilder<T>>(
                    methodName: nameof(InvokeGetSnapshot),
                    bindingFlags: BindingFlags.Static | BindingFlags.NonPublic,
                    genericTypes: new[] { propertyType }
                )
                .Invoke(null, new[] { value });

            return clone;
        }

        private static T InvokeGetSnapshot<T>(T instance)
        {
            return instance.GetSnapshot();
        }
    }
}