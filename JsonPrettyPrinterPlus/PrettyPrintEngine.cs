using System;
using System.IO;

namespace JsonPrettyPrinterPlus
{
    /// <summary>
    /// The formatter proper: one pass over the input, a switch per character, a stack of open scopes and a
    /// deferred line break. Line breaks are owed after <c>{</c>, <c>[</c> and <c>,</c> and written just before
    /// the next character, so a scope that closes at once prints as <c>{}</c> or <c>[]</c>.
    /// </summary>
    internal sealed class PrettyPrintEngine
    {
        private readonly string _newLine;
        private readonly string _indentUnit;

        private bool[] _scopeIsArray = new bool[16];
        private int _depth;

        private TextWriter _writer = TextWriter.Null;
        private char _previous;
        private bool _inDoubleQuotes;
        private bool _inSingleQuotes;
        private bool _escapeNext;
        private bool _pendingBreak;

        public PrettyPrintEngine(JsonPrettyPrintOptions options)
        {
            _newLine = options.NewLine;
            _indentUnit = options.IndentUnit;
        }

        /// <summary>Formats <paramref name="input"/> into <paramref name="writer"/>. State is reset first, so an engine can be reused.</summary>
        /// <exception cref="FormatException">A closing bracket has no matching opening bracket.</exception>
        public void Print(ReadOnlySpan<char> input, TextWriter writer)
        {
            _writer = writer;
            _depth = 0;
            _previous = '\0';
            _inDoubleQuotes = false;
            _inSingleQuotes = false;
            _escapeNext = false;
            _pendingBreak = false;

            for (var i = 0; i < input.Length; i++)
                Process(input[i], i);
        }

        private void Process(char c, int index)
        {
            if (_inDoubleQuotes || _inSingleQuotes)
            {
                ProcessInString(c);
                _previous = c;
                return;
            }

            switch (c)
            {
                case '{':
                case '[':
                    Write(c);
                    Push(c == '[');
                    _pendingBreak = true;
                    break;

                case '}':
                case ']':
                    Close(c, index);
                    break;

                case ',':
                    Write(c);
                    _pendingBreak = true;
                    break;

                case ':':
                    Write(c);
                    _writer.Write(' ');
                    break;

                case '"':
                    _inDoubleQuotes = true;
                    Write(c);
                    break;

                case '\'':
                    _inSingleQuotes = true;
                    Write(c);
                    break;

                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    // Dropped, and not recorded as the previous character, so "{ }" is still an empty object.
                    return;

                default:
                    Write(c);
                    break;
            }

            _previous = c;
        }

        private void ProcessInString(char c)
        {
            if (_escapeNext)
            {
                // A backslash escapes exactly the next character, so \\" is an escaped backslash then a closing quote.
                _escapeNext = false;
            }
            else if (c == '\\')
            {
                _escapeNext = true;
            }
            else if (c == '"' && _inDoubleQuotes)
            {
                _inDoubleQuotes = false;
            }
            else if (c == '\'' && _inSingleQuotes)
            {
                _inSingleQuotes = false;
            }

            Write(c);
        }

        private void Close(char c, int index)
        {
            if (_depth == 0)
                throw new FormatException($"Unexpected '{c}' at index {index}: there is no open object or array to close.");

            var expected = _scopeIsArray[_depth - 1] ? ']' : '}';
            if (c != expected)
                throw new FormatException($"Unexpected '{c}' at index {index}: expected '{expected}'.");

            _depth--;

            // An empty scope closes on the same line; anything else closes on a line of its own.
            _pendingBreak = _previous != '{' && _previous != '[';

            Write(c);
            _previous = c;
        }

        private void Push(bool isArray)
        {
            if (_depth == _scopeIsArray.Length)
                Array.Resize(ref _scopeIsArray, _depth * 2);

            _scopeIsArray[_depth++] = isArray;
        }

        private void Write(char c)
        {
            if (_pendingBreak)
                WriteBreak();

            _writer.Write(c);
        }

        private void WriteBreak()
        {
            _pendingBreak = false;
            _writer.Write(_newLine);

            if (_indentUnit.Length == 0)
                return;

            for (var i = 0; i < _depth; i++)
                _writer.Write(_indentUnit);
        }
    }
}
