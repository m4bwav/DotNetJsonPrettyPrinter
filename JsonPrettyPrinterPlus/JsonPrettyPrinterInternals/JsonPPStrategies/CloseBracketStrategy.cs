using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>}</c>: closes the object scope; an empty object prints as <c>{}</c>.</summary>
    public class CloseBracketStrategy : ICharacterStrategy
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
        public char ForWhichCharacter => '}';
    }
}
