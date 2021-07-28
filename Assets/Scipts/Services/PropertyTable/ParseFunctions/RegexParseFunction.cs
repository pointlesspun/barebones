
using System;
using System.Text.RegularExpressions;

namespace BareBones.Services.PropertyTable
{
    public abstract class RegexParseFunction : IPolyPropsParseFunction
    {
        public Action<(int, int), string> Log { get; set; }

        public Regex Matcher { get; set; }

        public abstract object Map(string text, int idx, int matchLength);

        public bool CanParse(string text, int start)
        {
            return Matcher.IsMatch(text, start);
        }

        public ParseResult Parse(string text, int start) => RegexParseFunction.Parse(text, start, Map, Matcher);

        public static ParseResult Parse(string text, int start, Func<string, int, int, object> mapFunction, Regex matcher)
        {
            var length = matcher.Match(text, start).Length;

            return new ParseResult(mapFunction(text, start, length), length, true);
        }
    }
}
