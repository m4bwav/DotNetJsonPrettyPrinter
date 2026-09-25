using System;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    /// <summary>Handles <c>{</c>: opens an object scope and owes a line break before its first member.</summary>
    public class OpenBracketStrategy : ICharacterStrategy
    {
        /// <inheritdoc />
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.AppendCurrentChar();

            if (context.IsProcessingString)
                return;

            context.EnterObjectScope();
            context.BuildContextIndents();
        }

        /// <inheritdoc />
        public char ForWhichCharacter => '{';
    }
}
