using System;

namespace BareBones.Services.PropertyTable
{
    public class StringParseFunction : AbstractParseFunction
    {
        public const string DefaultDelimiters = "\"'`";

        public const char DefaultEscapeChar = '\\';

        
        public string Delimiters { get; set; } = DefaultDelimiters;

        public char EscapeChar { get; set; } = DefaultEscapeChar;

        
        public override bool CanParse(string text, int start) => Delimiters.IndexOf(text[start]) >= 0;

        public override ParseResult Parse(string text, int start) => ParseString(text, start, Delimiters, EscapeChar, Log);

        public static ParseResult ParseString(
            string text, 
            int start, 
            string delimiters = DefaultDelimiters,
            char escapeChar = DefaultEscapeChar,  
            Action<(int, int), string> log = null)
        {
            var firstCharacter = text[start];

            if (delimiters.IndexOf(firstCharacter) >= 0)
            {
                var scopedStringLength = text.ReadScopedString(start, firstCharacter, escapeChar);

                if (scopedStringLength > 0)
                {
                    return new ParseResult(text.Substring(start + 1, scopedStringLength - 2), scopedStringLength, true);
                }

                log?.Invoke(text.GetLineAndColumn(start), "failed to parse string, syntax may be incorrect.");
                return new ParseResult(String.Empty, scopedStringLength, false);
            }

            log?.Invoke(text.GetLineAndColumn(start), "failed to parse string, missing beginning quotation mark.");
            return new ParseResult(String.Empty, start, false);
        }
    }
}
