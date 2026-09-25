using System.Collections.Generic;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals
{
    /// <summary>The stack of open objects and arrays. Public for 2.x compatibility only; internal in 3.0.</summary>
    public class PPScopeState
    {
        /// <summary>The kind of an open scope.</summary>
#pragma warning disable CA1720 // 'Object' is the JSON term; the name is public API kept until 3.0.
        public enum JsonScope
        {
            /// <summary>An object, opened by <c>{</c>.</summary>
            Object,

            /// <summary>An array, opened by <c>[</c>.</summary>
            Array
        }
#pragma warning restore CA1720

        private readonly Stack<JsonScope> _jsonScopeStack = new Stack<JsonScope>();

        /// <summary>True when the innermost open scope is an array.</summary>
        public bool IsTopTypeArray => _jsonScopeStack.Count > 0 && _jsonScopeStack.Peek() == JsonScope.Array;

        /// <summary>The number of open scopes.</summary>
        public int ScopeDepth => _jsonScopeStack.Count;

        /// <summary>Opens an object scope.</summary>
        public void PushObjectContextOntoStack()
        {
            _jsonScopeStack.Push(JsonScope.Object);
        }

        /// <summary>Closes the innermost scope and returns its kind.</summary>
        /// <exception cref="System.InvalidOperationException">No scope is open.</exception>
        public JsonScope PopJsonType()
        {
            return _jsonScopeStack.Pop();
        }

        /// <summary>Opens an array scope.</summary>
        public void PushJsonArrayType()
        {
            _jsonScopeStack.Push(JsonScope.Array);
        }

        /// <summary>Closes every scope.</summary>
        public void Clear()
        {
            _jsonScopeStack.Clear();
        }
    }
}
