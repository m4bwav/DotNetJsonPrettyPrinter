using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>,</c>: writes it and owes a line break before the next value.</summary>
    public class CommaStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.AppendCurrentChar();

            if (context.IsProcessingString)
                return;

            context.BuildContextIndents();
            context.IsProcessingVariableAssignment = false;
        }

        /// <inheritdoc />
        public char ForWhichCharacter => ',';
    }
}
