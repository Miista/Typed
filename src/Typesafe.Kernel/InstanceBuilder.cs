using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Kernel
{
    public interface IValueResolver<TInstance>
    {
        object Resolve(string parameterName);
    }
    
    public class InstanceBuilder<TInstance>
    {
        private readonly IValueResolver<TInstance> _valueResolver;

        public InstanceBuilder(IValueResolver<TInstance> valueResolver)
        {
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
        }

        public TInstance Create()
        {
            // 1. Create new instance
            var instance = CreateInstance(_valueResolver);

            // 2. Set properties (if needed)
            var enrichedInstance = SetProperties(instance, _valueResolver);
            
            return enrichedInstance;
        }

        private static TInstance SetProperties(TInstance instance, IValueResolver<TInstance> valueResolver)
        {
            var propertyInfos = typeof(TInstance)
                ?.GetProperties()
                ?.Where(info => info.CanWrite)
                ?.ToList();
            
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyValue = valueResolver.Resolve(propertyInfo.Name);
                propertyInfo.SetValue(instance, propertyValue);
            }

            return instance;
        }

        private static TInstance CreateInstance(IValueResolver<TInstance> properties)
        {
            var constructorParameters = typeof(TInstance)
                                            ?.GetConstructors()
                                            ?.OrderByDescending(info => info.GetParameters().Length)
                                            ?.FirstOrDefault()
                                            ?.GetParameters()
                                        ?? new ParameterInfo[0];

            var constructorParameterValues = new List<object>();
            
            foreach (var constructorParameter in constructorParameters)
            {
                var parameterValue = properties.Resolve(constructorParameter.Name);
                constructorParameterValues.Add(parameterValue);
            }

            if (Activator.CreateInstance(typeof(TInstance), constructorParameterValues.ToArray()) is TInstance instance) return instance;

            throw new Exception();
        }
    }
}