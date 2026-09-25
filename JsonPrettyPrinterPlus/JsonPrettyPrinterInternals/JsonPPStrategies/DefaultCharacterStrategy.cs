using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles every character with no strategy of its own: it is copied through unchanged.</summary>
    public class DefaultCharacterStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.AppendCurrentChar();
        }

        /// <summary>Not supported: this strategy is the fallback for every unregistered character.</summary>
        /// <exception cref="InvalidOperationException">Always.</exception>
        public char ForWhichCharacter =>
            throw new InvalidOperationException("This strategy was not intended for any particular character, so it has no one character");
    }
}
