
using System;

namespace BareBones.Services.PropertyTable
{
    public class KeywordParseFunction : AbstractParseFunction
    {
        public string Keyword { get; set; }

        public override bool CanParse(string text, int start) => text.IsMatch(Keyword, start, true);

        public Func<object> ValueFunction { get; set; }

        public override ParseResult Parse(string text, int start) 
            => KeywordParseFunction.Parse(text, start, Keyword, ValueFunction, OnMatch, Log);

        public static ParseResult Parse(
            string text, 
            int start, 
            string keyword,
            Func<object> valueFunction,
            Action<string> callback = null,
            Action<(int, int), string> log = null
        )
        {
            if (text.IsMatch(keyword, start, true))
            {
                callback?.Invoke(keyword);
                return new ParseResult(valueFunction != null ? valueFunction() : keyword, keyword.Length, true);
            }

            log?.Invoke(text.GetLineAndColumn(start), "Input at the given position does not match '" + keyword + "'.");
            return new ParseResult(null, 0, false);
        }            
    }
}
