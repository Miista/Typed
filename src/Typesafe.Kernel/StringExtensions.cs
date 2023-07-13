using System;

namespace Typesafe.Kernel
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Returns a copy of this string in parameter case.
        /// </summary>
        /// <param name="s">The string instance.</param>
        /// <returns>The parameter case equivalent of this string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
        public static string ToParameterCase(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var firstLetterInLowercase = char.ToLowerInvariant(s[0]);
            var remainingString = s.Substring(1);

            return firstLetterInLowercase + remainingString;
        }
        
        /// <summary>
        /// Returns a copy of this string in property case.
        /// </summary>
        /// <param name="s">The string instance.</param>
        /// <returns>The property case equivalent of this string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
        public static string ToPropertyCase(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var firstLetterInLowercase = char.ToUpperInvariant(s[0]);
            var remainingString = s.Substring(1);

            return firstLetterInLowercase + remainingString;
        }
    }
}