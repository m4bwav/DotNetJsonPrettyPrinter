using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>]</c>: closes the array scope; an empty array prints as <c>[]</c>.</summary>
    public class CloseSquareBracketStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.IsProcessingString)
            {
                context.AppendCurrentChar();
                return;
            }

            context.CloseCurrentScope();

            if (context.WasLastCharacterAnOpenBracket)
                context.RemoveTrailingIndent();
            else
                context.BuildContextIndents();

            context.AppendCurrentChar();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => ']';
    }
}
