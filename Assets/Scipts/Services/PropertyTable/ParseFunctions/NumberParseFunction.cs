using System;

namespace BareBones.Services.PropertyTable
{
    public class NumberParseFunction : IParseFunction
    {
        public const string DefaultDelimiters = " \t\n\r,[]{}/";

        public Action<(int, int), string> Log { get; set; }

        public string Delimiters { get; set; } = DefaultDelimiters;

        public string AllowedTypes { get; set; } = null;

        public bool CanParse(string text, int start) => text[start] == '-' || char.IsDigit(text[start]);

        public ParseResult Parse(string text, int start) => NumberParseFunction.Parse(text, start, Delimiters, AllowedTypes, Log);


        public static ParseResult Parse(string text, int start, string delimiters = DefaultDelimiters, string allowedTypes = null, Action<(int, int), string> log = null)
        {
            var idx = ParseUtil.SkipUntil(text, start, (chr) => delimiters.IndexOf(chr) >= 0);

            if (idx > start)
            {
                var numberString = text.Substring(start, idx - start);

                try
                {
                    return new ParseResult(allowedTypes == null 
                                            ? numberString.ParseNumber() 
                                            : numberString.ParseNumber(allowedTypes), idx - start, true);
                }
                catch (Exception e)
                {
                    log?.Invoke(text.GetLineAndColumn(start), "failed to parse number. Exception: " + e);
                    return new ParseResult(null, idx - start, false);
                }
            }

            log?.Invoke(text.GetLineAndColumn(start), "trying to parse a number but the end of the input was reached.");
            return new ParseResult(null, start, false);
        }
    }
}
