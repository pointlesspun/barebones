
using System;

namespace BareBones.Services.PropertyTable
{
    public class KeywordParseFunction : IPolyPropsParseFunction
    {
        public Action<(int, int), string> Log { get; set; }

        public string Keyword { get; set; }

        public bool CanParse(string text, int start) => text.IsMatch(Keyword, start, true);

        public Func<object> ValueFunction { get; set; }

        public ParseResult Parse(string text, int start) => KeywordParseFunction.Parse(text, start, Keyword, ValueFunction, Log);


        public static ParseResult Parse(
            string text, 
            int start, 
            string keyword,
            Func<object> valueFunction,
            Action<(int, int), string> log = null
        )
        {
            if (text.IsMatch(keyword, start, true))
            {
                return new ParseResult(valueFunction(), keyword.Length, true);
            }

            log?.Invoke(text.GetLineAndColumn(start), "Input at the given position does not match '" + keyword + "'.");
            return new ParseResult(null, 0, false);
        }            
    }
}
