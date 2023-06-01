using System;
using System.Reflection;

namespace Typesafe.Snapshots.Cloner
{
    internal class InterfaceCloner<TInterface> : ITypeCloner<TInterface> 
    {
        public TInterface Clone(TInterface instance)
        {
            return (TInterface)CloneValue(instance.GetType(), instance);
        }
        
        private static object CloneValue(Type propertyType, object value)
        {
            var clone = ReflectionHelper.MakeGenericMethod<InterfaceCloner<TInterface>>(
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