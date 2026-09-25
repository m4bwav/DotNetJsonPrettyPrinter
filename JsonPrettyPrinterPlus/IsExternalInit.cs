#if !NET5_0_OR_GREATER
// Lets the compiler emit init-only setters (used by JsonPrettyPrintOptions) when targeting netstandard2.0.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
#endif
