using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>:</c>: writes it followed by one space.</summary>
    public class ColonCharacterStrategy : ICharacterStrategy
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

            context.IsProcessingVariableAssignment = true;
            context.AppendCurrentChar();
            context.AppendSpace();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => ':';
    }
}
