using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles one whitespace character: dropped outside strings, kept inside them.</summary>
    public class SkipWhileNotInStringStrategy : ICharacterStrategy
    {
        private readonly char _selectionCharacter;

        /// <summary>Creates a strategy for <paramref name="selectionCharacter"/>.</summary>
        public SkipWhileNotInStringStrategy(char selectionCharacter)
        {
            _selectionCharacter = selectionCharacter;
        }

        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.IsProcessingString)
                context.AppendCurrentChar();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => _selectionCharacter;
    }
}
