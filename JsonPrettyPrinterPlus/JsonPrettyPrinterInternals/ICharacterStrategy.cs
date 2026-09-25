namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals
{
    /// <summary>Handles one input character. Public for 2.x compatibility only; internal in 3.0.</summary>
    public interface ICharacterStrategy
    {
        /// <summary>The character this strategy is registered for.</summary>
        char ForWhichCharacter { get; }

        /// <summary>Writes the current character of <paramref name="context"/> and updates its state.</summary>
        void ExecutePrintyPrint(JsonPPStrategyContext context);
    }
}
