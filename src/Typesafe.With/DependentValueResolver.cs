using System.Reflection;

namespace Typesafe.With
{
    internal class DependentValueResolver<TInstance>
    {
        private readonly TInstance _instance;

        public DependentValueResolver(TInstance instance)
        {
            _instance = instance;
        }

        public object Resolve(DependentValue dependentValue, PropertyInfo existingProperty)
        {
            return dependentValue.Resolve(existingProperty, _instance);
        }
    }
}