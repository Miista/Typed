using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typesafe.Kernel;

namespace Typesafe.Snapshots
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
            var (constructedInstance, remainingPropertiesAfterCtor) = WithByConstructor(instance, properties, _constructorInfo);
            
            /*// 2. Set new properties via property setters
            var (enrichedInstance, remainingPropertiesAfterPropSet) = EnrichByProperty(constructedInstance, remainingPropertiesAfterCtor, valueResolver, valueCloner);

            if (remainingPropertiesAfterPropSet.Count != 0)
            {
                throw new InvalidOperationException("There are still properties to set but no way to set them.");
            }*/
            
            // 2. Copy remaining properties
            var copyProperties = GetCopyProperties(_constructorInfo, properties);
            var enrichedInstanceWithCopiedProperties = CopyProperties(instance, constructedInstance, copyProperties);
            
            return enrichedInstanceWithCopiedProperties;
        }

        private static (TInstance Instance, IReadOnlyDictionary<string, object> RemainingProperties) WithByConstructor<TInstance>(
            TInstance instance,
            IReadOnlyDictionary<string, object> newProperties,
            ConstructorInfo constructorInfo
        )
        {
            var existingProperties = (IDictionary<string, PropertyInfo>) TypeUtils.GetPropertyDictionary<TInstance>();

            var remainingProperties = new Dictionary<string, object>(newProperties.ToDictionary(pair => pair.Key, pair => pair.Value));
            var parameters = new List<object>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                var (existingProperty, propertyName) = TryFindExistingProperty(parameter);
                var originalValue = existingProperty?.GetValue(instance);
                var clone = typeof(TypeBuilder<T>)
                    .GetMethod(nameof(TypeBuilder<T>.CloneValue), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(existingProperty.PropertyType)
                    .Invoke(null, new object[]{originalValue});
                var value = clone;

                parameters.Add(value);
                remainingProperties.Remove(propertyName);
            }

            var constructedInstance = constructorInfo.Invoke(parameters.ToArray()) is TInstance
                ? (TInstance) constructorInfo.Invoke(parameters.ToArray())
                : throw new InvalidOperationException($"Cannot construct instance of type {typeof(TInstance)}");

            return (constructedInstance, remainingProperties);
            
            (PropertyInfo ExistingProperty, string PropertyName) TryFindExistingProperty(ParameterInfo parameterInfo)
            {
                // Can we find a matching constructor parameter?
                if (existingProperties.TryGetValue(parameterInfo.Name, out var existingPropertyByExactMatch))
                {
                    return (existingPropertyByExactMatch, parameterInfo.Name);
                }

                // Can we find a matching constructor parameter if we lowercase both parameter and property name?
                var existingPropertyKey =
                    existingProperties.Keys.FirstOrDefault(key => string.Equals(key, parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    ?? throw new InvalidOperationException($"Cannot find property for constructor parameter '{parameterInfo.Name}'.");

                var existingPropertyByLowercaseMatch = existingProperties[existingPropertyKey];
                
                return (existingPropertyByLowercaseMatch, existingPropertyKey);
            }
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

        private static T CopyProperties(T source, T destination, IEnumerable<PropertyInfo> properties)
        {
            foreach (var kvp in properties)
            {
                var value = kvp.GetValue(source);

                var clone = MakeGenericMethod<TypeBuilder<T>>(
                        methodName: nameof(TypeBuilder<T>.CloneValue),
                        bindingFlags: BindingFlags.Static | BindingFlags.NonPublic,
                        genericTypes: new[] { kvp.PropertyType }
                    )
                    .Invoke(null, new object[] { value });
                kvp.SetValue(destination, clone);
            }

            return destination;
        }

        private static MethodInfo MakeGenericMethod<TT>(
            string methodName,
            BindingFlags bindingFlags,
            Type[] genericTypes
        )
        {
            return typeof(TT)
                       ?.GetMethod(methodName, bindingFlags)
                       ?.MakeGenericMethod(genericTypes)
                   ?? throw new Exception($"Cannot make generic method '{methodName}'");
        }
        
        private static T CloneValue<T>(T instance)
        {
            return instance.GetSnapshot();
        }
    }
}