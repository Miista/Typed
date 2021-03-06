﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Kernel
{
    internal static class TypeUtils
    {
        public static Dictionary<string, PropertyInfo> GetPropertyDictionary<T>() =>
            typeof(T)
                .GetProperties()
                .ToDictionary(info => info.Name.ToParameterCase());

        public static ConstructorInfo GetSuitableConstructor<T>() =>
            typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault()
            ?? throw new InvalidOperationException($"Could not find any constructor for type {typeof(T)}.");
    }
}