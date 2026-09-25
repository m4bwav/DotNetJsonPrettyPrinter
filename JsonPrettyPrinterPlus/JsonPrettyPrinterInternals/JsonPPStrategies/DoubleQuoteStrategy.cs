using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>"</c>: starts or ends a string. Escaped quotes never reach this strategy; the context consumes escapes first.</summary>
    public class DoubleQuoteStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.IsProcessingSingleQuoteInitiatedString)
                context.IsProcessingDoubleQuoteInitiatedString = !context.IsProcessingDoubleQuoteInitiatedString;

            context.AppendCurrentChar();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => '"';
    }
}
