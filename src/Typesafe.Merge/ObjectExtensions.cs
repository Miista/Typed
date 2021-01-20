using System;
using Typesafe.Kernel;

namespace Typesafe.Merge
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Merges <paramref name="right"/> into <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The left side of the merge.</param>
        /// <param name="right">The right side of the merge.</param>
        /// <typeparam name="T">The type on both sides of the merge.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        public static T Merge<T>(this T left, T right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            var valueResolver = new MergeValueResolver<T, T, T>(left, right);
            var instanceBuilder = new InstanceBuilder<T>(valueResolver);
            
            return instanceBuilder.Construct();
        }
        
        /// <summary>
        /// Merges <paramref name="right"/> into <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The left side of the merge.</param>
        /// <param name="right">The right side of the merge.</param>
        /// <typeparam name="TDestination">The type of the merged instance.</typeparam>
        /// <typeparam name="TLeft">The type on the left side of the merge.</typeparam>
        /// <typeparam name="TRight">The type on the right side of the merge.</typeparam>
        /// <returns>A new instance of <typeparamref name="TDestination"/>.</returns>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        public static TDestination Merge<TDestination, TLeft, TRight>(this TLeft left, TRight right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            var valueResolver = new MergeValueResolver<TDestination, TLeft, TRight>(left, right);
            var instanceBuilder = new InstanceBuilder<TDestination>(valueResolver);
            
            return instanceBuilder.Construct();
        }
    }
}