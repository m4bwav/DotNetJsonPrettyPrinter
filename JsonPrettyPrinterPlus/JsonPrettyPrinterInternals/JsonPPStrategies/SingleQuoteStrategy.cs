using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>'</c>: starts or ends a single-quoted string (not JSON, but tolerated). Escapes are consumed by the context first.</summary>
    public class SingleQuoteStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.IsProcessingDoubleQuoteInitiatedString)
                context.IsProcessingSingleQuoteInitiatedString = !context.IsProcessingSingleQuoteInitiatedString;

            context.AppendCurrentChar();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => '\'';
    }
}
