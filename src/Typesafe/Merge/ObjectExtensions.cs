using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Typesafe.Tests")]
[assembly: InternalsVisibleTo("Typesafe.Sandbox")]
namespace Typesafe.Merge
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Merges <paramref name="right"/> into <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The left side of the merge.</param>
        /// <param name="right">The right side of the merge.</param>
        /// <typeparam name="T">The type on both sides of the merge.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        public static T Merge<T>(T left, T right)
            where T : class
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            var builder = new UnifiedMergeBuilder();
            
            return builder.Construct<T, T, T>(left, right);
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
        public static TDestination Merge<TDestination, TLeft, TRight>(TLeft left, TRight right)
            where TDestination : class
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
        
            var builder = new UnifiedMergeBuilder();
            
            return builder.Construct<TDestination, TLeft, TRight>(left, right);
        }
    }
}