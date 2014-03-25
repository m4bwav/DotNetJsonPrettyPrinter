using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonPrettyPrinterPlus.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class OpenBracketStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context)
        {
            if (context.IsProcessingString)
            {
                context.AppendCurrentChar();
                return;
            }

            context.AppendCurrentChar();
            context.EnterObjectScope();

            if (!IsBeginningOfNewLineAndIndentionLevel(context)) return;

            context.BuildContextIndents();
        }

        private static bool IsBeginningOfNewLineAndIndentionLevel(JsonPPStrategyContext context)
        {
            return context.IsProcessingVariableAssignment || (!context.IsStart && !context.IsInArrayScope);
        }

        public char ForWhichCharacter
        {
            get { return '{'; }
        }
    }
}
