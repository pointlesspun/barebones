using System;

namespace BareBones.Services.PropertyTable
{
    public class AnyCharParseFunction : IPolyPropsParseFunction
    {
        public const string DefaultDelimiters = "\n\r/:[]{},";

        public const char DefaultEscapeChar = '\\';

        public Action<(int, int), string> Log { get; set; }

        public string Delimiters { get; set; } = DefaultDelimiters;

        public char EscapeChar { get; set; } = DefaultEscapeChar;


        public bool CanParse(string text, int start) => Delimiters.IndexOf(text[start]) < 0;

        public ParseResult Parse(string text, int start) => ParseAnyChar(text, start, Delimiters, EscapeChar, Log);

        public static ParseResult ParseAnyChar(
            string text,
            int start,
            string delimiters = DefaultDelimiters,
            char escapeChar = DefaultEscapeChar,
            Action<(int, int), string> log = null)
        {
            var idx = start;

            while (idx < text.Length && delimiters.IndexOf(text[idx]) < 0)
            {
                if (text[idx] == escapeChar)
                {
                    idx++;
                }

                idx++;
            }

            return new ParseResult(text.Substring(start, idx - start), idx - start, true);
        }
    }
}
