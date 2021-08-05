
using System;
using System.Text.RegularExpressions;

namespace BareBones.Services.PropertyTable
{
    public abstract class RegexParseFunction : AbstractParseFunction
    {
    
        public Regex Matcher { get; set; }

        public abstract object Map(string text, int idx, int matchLength);

        public override bool CanParse(string text, int start) 
            =>  Matcher.IsMatch(text, start);

        public override ParseResult Parse(string text, int start = 0) => RegexParseFunction.Parse(text, start, Map, Matcher);

        public static ParseResult Parse(string text, int start, Func<string, int, int, object> mapFunction, Regex matcher)
        {
            var length = matcher.Match(text, start).Length;

            return new ParseResult(mapFunction(text, start, length), length, true);
        }
    }
}
