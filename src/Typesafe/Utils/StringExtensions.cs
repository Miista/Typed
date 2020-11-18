using System;

namespace Typesafe.Utils
{
    internal static class StringExtensions
    {
        public static string ToPropertyCase(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var firstLetterInLowercase = char.ToLowerInvariant(s[0]);
            var remainingString = s.Substring(1);

            return firstLetterInLowercase + remainingString;
        }
    }
}