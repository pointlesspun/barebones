
using System;

namespace BareBones.Services.PropertyTable
{
    public class BooleanParseFunction : IPolyPropsParseFunction
    {
        public const string DefaultTrueToken = "true";
        public const string DefaultFalseToken = "false";

        public Action<(int, int), string> Log { get; set; }

        public string TrueToken { get; set; } = DefaultTrueToken;

        public string FalseToken { get; set; } = DefaultFalseToken;

        public bool CanParse(string text, int start) =>
            text.IsMatch(TrueToken, start, true) || text.IsMatch(FalseToken, start, true);

        public ParseResult Parse(string text, int start) => BooleanParseFunction.Parse(text, start, TrueToken, FalseToken, Log);

        public static ParseResult Parse(
            string text, 
            int start, 
            string trueToken = DefaultTrueToken, 
            string falseToken = DefaultFalseToken, 
            Action<(int, int), string> log = null
        )
        {
            if (text.IsMatch(trueToken, start, true))
            {
                return new ParseResult(true, trueToken.Length, true);
            }
            else if (text.IsMatch(falseToken, start, true))
            {
                return new ParseResult(false, falseToken.Length, true);
            }

            log?.Invoke(text.GetLineAndColumn(start), "failed to parse bool. Input at the given position matches neither '" + trueToken + "' nor '" + falseToken + ".");
            return new ParseResult(null, 0, false);
        }            
    }
}
