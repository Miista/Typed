using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typesafe.Kernel
{
    internal static class TypeUtils
    {
        /// <summary>
        /// Returns all the public properties of <typeparamref name="T"/> with the property names in parameter case.
        /// </summary>
        /// <typeparam name="T">The type to return the properties for.</typeparam>
        /// <seealso cref="StringExtensions.ToParameterCase"/>
        /// <returns>A dictionary whose keys are the property names in parameter case and whose values are the properties of type <typeparamref name="T"/>.</returns>
        public static Dictionary<string, PropertyInfo> GetPropertyDictionary<T>() =>
            typeof(T)
                .GetProperties()
                .ToDictionary(info => info.Name.ToParameterCase());

        /// <summary>
        /// Returns the <see cref="ConstructorInfo"/> for <typeparamref name="T"/> which takes the most parameters.
        /// </summary>
        /// <typeparam name="T">The type to return the <see cref="ConstructorInfo"/> for.</typeparam>
        /// <returns>A <see cref="ConstructorInfo"/> suitable for constructing an instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">There are no suitable constructors.</exception>
        public static ConstructorInfo GetSuitableConstructor<T>() =>
            typeof(T)
                .GetConstructors()
                .OrderByDescending(info => info.GetParameters().Length)
                .FirstOrDefault()
            ?? throw new InvalidOperationException($"Could not find any constructor for type {typeof(T)}.");
    }
}