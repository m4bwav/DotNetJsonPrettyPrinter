namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals
{
    public interface ICharacterStrategy
    {
        void ExecutePrintyPrint(JsonPPStrategyContext context);
        char ForWhichCharacter { get; }
    }
}
