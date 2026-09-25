using System;

namespace JsonPrettyPrinterPlus
{
    /// <summary>
    /// Controls how <see cref="JsonPrettyPrinter"/> lays out its output. The default instance reproduces the
    /// library's historical output: four spaces per level and <see cref="Environment.NewLine"/> between lines.
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new JsonPrettyPrintOptions { IndentSize = 2, NewLine = "\n" };
    /// var pretty = json.PrettyPrintJson(options);
    /// </code>
    /// </example>
    public sealed record JsonPrettyPrintOptions
    {
        private readonly int _indentSize = 4;
        private readonly string _newLine = Environment.NewLine;

        /// <summary>The options used when none are given: four spaces, <see cref="Environment.NewLine"/>.</summary>
        public static JsonPrettyPrintOptions Default { get; } = new JsonPrettyPrintOptions();

        /// <summary>Spaces written per nesting level. Zero means no indentation. Ignored when <see cref="UseTabs"/> is set.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
        public int IndentSize
        {
            get => _indentSize;
            init
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "IndentSize cannot be negative.");
                _indentSize = value;
            }
        }

        /// <summary>Indent with one tab per nesting level instead of <see cref="IndentSize"/> spaces.</summary>
        public bool UseTabs { get; init; }

        /// <summary>
        /// The line terminator. Defaults to <see cref="Environment.NewLine"/> so 2.x output is unchanged;
        /// pass <c>"\n"</c> for output that is the same on every OS.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        public string NewLine
        {
            get => _newLine;
            init => _newLine = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>The string written once per nesting level.</summary>
        internal string IndentUnit => UseTabs ? "\t" : IndentSize == 0 ? string.Empty : new string(' ', IndentSize);
    }
}
