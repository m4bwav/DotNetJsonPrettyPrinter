using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>[</c>: opens an array scope and owes a line break before its first element.</summary>
    public class OpenSquareBracketStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.AppendCurrentChar();

            if (context.IsProcessingString)
                return;

            context.EnterArrayScope();
            context.BuildContextIndents();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => '[';
    }
}
