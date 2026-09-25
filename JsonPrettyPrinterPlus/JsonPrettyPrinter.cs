using System;
using System.IO;
using System.Text;

namespace JsonPrettyPrinterPlus
{
    /// <summary>Extension methods that pretty print a JSON string in one call.</summary>
    public static class PrettyPrinterExtensions
    {
        [ThreadStatic] private static JsonPrettyPrinter? t_defaultPrinter;

        /// <summary>Pretty prints <paramref name="unprettyJson"/> with <see cref="JsonPrettyPrintOptions.Default"/>.</summary>
        /// <param name="unprettyJson">The JSON text. Whitespace outside strings is discarded and rewritten.</param>
        /// <returns>The indented text, or an empty string when the input is blank.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unprettyJson"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public static string PrettyPrintJson(this string unprettyJson)
        {
            var printer = t_defaultPrinter ??= new JsonPrettyPrinter();
            return printer.PrettyPrint(unprettyJson);
        }

        /// <summary>Pretty prints <paramref name="unprettyJson"/> with the given layout options.</summary>
        /// <param name="unprettyJson">The JSON text. Whitespace outside strings is discarded and rewritten.</param>
        /// <param name="options">Indent and newline settings.</param>
        /// <returns>The indented text, or an empty string when the input is blank.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unprettyJson"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public static string PrettyPrintJson(this string unprettyJson, JsonPrettyPrintOptions options)
        {
            return new JsonPrettyPrinter(options).PrettyPrint(unprettyJson);
        }
    }

    /// <summary>
    /// Indents JSON text one value per line. It is a formatter, not a validator: it tracks strings, escapes and
    /// bracket nesting and leaves everything else as it finds it, so trailing commas, comments or single-quoted
    /// strings pass through untouched. Instances are not thread-safe; create one per thread or use the
    /// <see cref="PrettyPrinterExtensions"/> methods, which keep one per thread.
    /// </summary>
    public sealed class JsonPrettyPrinter
    {
        private readonly PrettyPrintEngine _engine;

        /// <summary>Creates a printer with <see cref="JsonPrettyPrintOptions.Default"/>.</summary>
        public JsonPrettyPrinter() : this(JsonPrettyPrintOptions.Default)
        {
        }

        /// <summary>Creates a printer with the given layout options.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        public JsonPrettyPrinter(JsonPrettyPrintOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _engine = new PrettyPrintEngine(options);
        }

        /// <summary>The layout options this printer writes with.</summary>
        public JsonPrettyPrintOptions Options { get; }

        /// <summary>Pretty prints <paramref name="inputString"/>.</summary>
        /// <returns>The indented text, or an empty string when the input is blank.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputString"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public string PrettyPrint(string inputString)
        {
            if (inputString == null)
                throw new ArgumentNullException(nameof(inputString));

            return PrettyPrint(inputString.AsSpan());
        }

        /// <summary>Pretty prints <paramref name="input"/> without first copying it into a string.</summary>
        /// <returns>The indented text, or an empty string when the input is blank.</returns>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public string PrettyPrint(ReadOnlySpan<char> input)
        {
            if (input.Trim().IsEmpty)
                return string.Empty;

            var output = new StringBuilder(input.Length + input.Length / 2);
            using (var writer = new StringWriter(output))
            {
                _engine.Print(input, writer);
            }

            return output.ToString();
        }

        /// <summary>Pretty prints <paramref name="inputString"/> straight into <paramref name="output"/>, so large documents never need a second string.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="inputString"/> or <paramref name="output"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public void PrettyPrint(string inputString, TextWriter output)
        {
            if (inputString == null)
                throw new ArgumentNullException(nameof(inputString));

            PrettyPrint(inputString.AsSpan(), output);
        }

        /// <summary>Pretty prints <paramref name="input"/> straight into <paramref name="output"/>. Blank input writes nothing.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="output"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public void PrettyPrint(ReadOnlySpan<char> input, TextWriter output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (input.Trim().IsEmpty)
                return;

            _engine.Print(input, output);
        }
    }
}
