using System;
using System.Collections.Generic;
using Typesafe.Kernel;

namespace Typesafe.Snapshots
{
    public static class ObjectExtensions
    {
        public static T GetSnapshot<T>(this T self)
        {
            var constructor = TypeUtils.GetSuitableConstructor<T>();
            var typeBuilder = new TypeBuilder<T>(constructor);
            var snapshot = typeBuilder.Construct(self, new Dictionary<string, object>());
            
            return snapshot;
        }
    }
}