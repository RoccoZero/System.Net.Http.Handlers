#if !NET6_0_OR_GREATER
namespace System;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class ArgumentNullExceptionExtensions
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
#endif