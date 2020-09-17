using System;

namespace FplManager.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ConcatWithNewLine(this string str, string concatString) => $"{str}{concatString}{Environment.NewLine}";
    }
}
