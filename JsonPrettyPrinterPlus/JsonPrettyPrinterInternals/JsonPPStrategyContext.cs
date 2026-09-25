using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals
{
    /// <summary>
    /// The state shared by the character strategies while a document is printed: the output sink, the scope
    /// stack, string and escape tracking, and the line break owed after an opening bracket or comma. Public
    /// for 2.x compatibility only; this type becomes internal in 3.0.
    /// </summary>
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The StringWriter wraps a caller-owned StringBuilder and holds no unmanaged resources.")]
    public class JsonPPStrategyContext
    {
        private const string Space = " ";
        private static readonly DefaultCharacterStrategy DefaultStrategy = new DefaultCharacterStrategy();

        private readonly PPScopeState _scopeState = new PPScopeState();
        private readonly Dictionary<char, ICharacterStrategy> _strategyCatalog = new Dictionary<char, ICharacterStrategy>();

#pragma warning disable CA1051 // Public fields kept for 2.x binary compatibility; removed in 3.0.
        /// <summary>Set by the colon strategy and cleared by the comma strategy. Nothing reads it any more; removed in 3.0.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsProcessingVariableAssignment;

        /// <summary>Spaces per nesting level. Initialised from <see cref="Options"/>; prefer <see cref="JsonPrettyPrintOptions.IndentSize"/>. Removed in 3.0.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int SpacesPerIndent;
#pragma warning restore CA1051

        private string _indent = string.Empty;
        private int _indentBuiltFor = -1;

        private TextWriter? _writer;
        private StringWriter? _builderWriter;
        private StringBuilder? _builder;
        private long _written;

        private char _currentCharacter;
        private char _previousChar;
        private int _position;
        private bool _escapeNext;
        private bool _pendingBreak;

        /// <summary>Creates a context with <see cref="JsonPrettyPrintOptions.Default"/>.</summary>
        public JsonPPStrategyContext() : this(JsonPrettyPrintOptions.Default)
        {
        }

        /// <summary>Creates a context that lays out with <paramref name="options"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        public JsonPPStrategyContext(JsonPrettyPrintOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            SpacesPerIndent = options.IndentSize;
        }

        /// <summary>The layout options in force.</summary>
        public JsonPrettyPrintOptions Options { get; }

        /// <summary>The text written once per nesting level.</summary>
        public string Indent
        {
            get
            {
                if (Options.UseTabs)
                    return "\t";

                if (SpacesPerIndent <= 0)
                    return string.Empty;

                if (_indentBuiltFor != SpacesPerIndent)
                {
                    _indent = new string(' ', SpacesPerIndent);
                    _indentBuiltFor = SpacesPerIndent;
                }

                return _indent;
            }
        }

        /// <summary>True when the innermost open scope is an array.</summary>
        public bool IsInArrayScope => _scopeState.IsTopTypeArray;

        /// <summary>True between an opening and closing double quote.</summary>
        public bool IsProcessingDoubleQuoteInitiatedString { get; set; }

        /// <summary>True between an opening and closing single quote (not JSON, but tolerated).</summary>
        public bool IsProcessingSingleQuoteInitiatedString { get; set; }

        /// <summary>True inside any string literal.</summary>
        public bool IsProcessingString => IsProcessingDoubleQuoteInitiatedString || IsProcessingSingleQuoteInitiatedString;

        /// <summary>True until the first character has been written.</summary>
        public bool IsStart => _written == 0;

        /// <summary>True when the last significant (non-whitespace) character was an opening bracket, i.e. the scope is empty so far.</summary>
        public bool WasLastCharacterAnOpenBracket => _previousChar == '{' || _previousChar == '[';

        /// <summary>True when the previous character was a backslash. Escapes are now handled before dispatch, so strategies no longer need this. Removed in 3.0.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool WasLastCharacterABackSlash => _previousChar == '\\';

        /// <summary>
        /// Cancels the line break owed after an opening bracket so an empty scope prints as <c>{}</c> or <c>[]</c>.
        /// When the output is a <see cref="StringBuilder"/>, trailing spaces already written are removed as well.
        /// </summary>
        public void RemoveTrailingIndent()
        {
            _pendingBreak = false;

            if (_builder == null)
                return;

            var length = _builder.Length;
            while (length > 0 && _builder[length - 1] == ' ') length--;
            _builder.Length = length;
        }

        /// <summary>Clears all per-document state so the context can print another document.</summary>
        public void Reset()
        {
            _scopeState.Clear();
            IsProcessingDoubleQuoteInitiatedString = false;
            IsProcessingSingleQuoteInitiatedString = false;
            IsProcessingVariableAssignment = false;
            _previousChar = '\0';
            _currentCharacter = '\0';
            _position = 0;
            _written = 0;
            _escapeNext = false;
            _pendingBreak = false;
        }

        /// <summary>Prints one character into <paramref name="output"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="output"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public void PrettyPrintCharacter(char curChar, StringBuilder output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (!ReferenceEquals(_builder, output))
            {
                _builder = output;
                _builderWriter = new StringWriter(output);
            }

            PrettyPrintCharacter(curChar, _builderWriter!);
        }

        /// <summary>Prints one character into <paramref name="output"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="output"/> is null.</exception>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public void PrettyPrintCharacter(char curChar, TextWriter output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (!ReferenceEquals(_writer, output))
            {
                _writer = output;
                if (!ReferenceEquals(_builderWriter, output))
                    _builder = null;
            }

            _currentCharacter = curChar;

            if (IsProcessingString && (_escapeNext || curChar == '\\'))
            {
                // A backslash escapes exactly the next character, whatever it is, so \\" is an escaped
                // backslash followed by a closing quote and \" is an escaped quote.
                _escapeNext = !_escapeNext;
                Write(curChar);
            }
            else
            {
                var strategy = _strategyCatalog.TryGetValue(curChar, out var found) ? found : DefaultStrategy;
                strategy.ExecutePrintyPrint(this);
            }

            // Skipped whitespace must not count as the previous character, or "{ }" is not seen as empty.
            if (IsProcessingString || !IsJsonWhitespace(curChar))
                _previousChar = curChar;

            _position++;
        }

        /// <summary>Writes the character being processed.</summary>
        public void AppendCurrentChar()
        {
            Write(_currentCharacter);
        }

        /// <summary>Writes <see cref="JsonPrettyPrintOptions.NewLine"/> immediately.</summary>
        public void AppendNewLine()
        {
            FlushPendingBreak();
            WriteString(Options.NewLine);
        }

        /// <summary>
        /// Requests a line break followed by the indent for the current depth. It is written just before the next
        /// character, so a scope that closes immediately can cancel it with <see cref="RemoveTrailingIndent"/>.
        /// </summary>
        public void BuildContextIndents()
        {
            _pendingBreak = true;
        }

        /// <summary>Pushes an object scope.</summary>
        public void EnterObjectScope()
        {
            _scopeState.PushObjectContextOntoStack();
        }

        /// <summary>Pops the innermost scope, checking that the current character closes it.</summary>
        /// <exception cref="FormatException">There is no open scope, or the open scope is of the other kind.</exception>
        public void CloseCurrentScope()
        {
            if (_scopeState.ScopeDepth == 0)
                throw new FormatException($"Unexpected '{_currentCharacter}' at index {_position}: there is no open object or array to close.");

            var expected = _scopeState.IsTopTypeArray ? ']' : '}';
            if ((_currentCharacter == '}' || _currentCharacter == ']') && _currentCharacter != expected)
                throw new FormatException($"Unexpected '{_currentCharacter}' at index {_position}: expected '{expected}'.");

            _scopeState.PopJsonType();
        }

        /// <summary>Pushes an array scope.</summary>
        public void EnterArrayScope()
        {
            _scopeState.PushJsonArrayType();
        }

        /// <summary>Writes a single space.</summary>
        public void AppendSpace()
        {
            WriteString(Space);
        }

        /// <summary>Removes every registered strategy.</summary>
        public void ClearStrategies()
        {
            _strategyCatalog.Clear();
        }

        /// <summary>Registers <paramref name="strategy"/> for its character, replacing any earlier one.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="strategy"/> is null.</exception>
        public void AddCharacterStrategy(ICharacterStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            _strategyCatalog[strategy.ForWhichCharacter] = strategy;
        }

        private static bool IsJsonWhitespace(char c)
        {
            return c == ' ' || c == '\n' || c == '\r' || c == '\t';
        }

        private void Write(char c)
        {
            FlushPendingBreak();
            _writer!.Write(c);
            _written++;
        }

        private void WriteString(string s)
        {
            _writer!.Write(s);
            _written += s.Length;
        }

        private void FlushPendingBreak()
        {
            if (!_pendingBreak)
                return;

            _pendingBreak = false;
            WriteString(Options.NewLine);

            var indent = Indent;
            if (indent.Length == 0)
                return;

            for (var depth = _scopeState.ScopeDepth; depth > 0; depth--)
                WriteString(indent);
        }
    }
}
