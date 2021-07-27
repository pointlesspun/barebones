
using BareBones.Common;
using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class GroupParseFunction : IPolyPropsParseFunction
    {
        public Action<(int, int), string> Log { get; set; }

        public List<IPolyPropsParseFunction> ParseFunctions { get; private set; } = new List<IPolyPropsParseFunction>();

        public IPolyPropsParseFunction DefaultFunction { get; set; } = null;

        public Func<string, int, int> SkipWhiteSpaceFunction { get; set; }

        public GroupParseFunction Add(params IPolyPropsParseFunction[] functions)
        {
            ParseFunctions.AddRange(functions);
            return this;
        }

        public bool CanParse(string text, int start = 0) =>
            start < text.Length
            &&  ParseFunctions.Any(f => f.CanParse(text, start)) 
            || (DefaultFunction != null && DefaultFunction.CanParse(text, start));

        public ParseResult Parse(string text, int start = 0)
        {
            var idx = start;

            if (SkipWhiteSpaceFunction != null)
            {
                idx = SkipWhiteSpaceFunction(text, idx);
            }

            if (!String.IsNullOrEmpty(text) && idx < text.Length)
            {
                var f = ParseFunctions.FirstOrDefault(f => f.CanParse(text, idx));

                if (f == null)
                {
                    if (DefaultFunction != null)
                    {
                        return DefaultFunction.Parse(text, idx);
                    }
                    else
                    {
                        Log?.Invoke(text.GetLineAndColumn(idx),
                            "trying to parse a text starting with the character: " + text[idx] + "," +
                                "however this character does not match the CanParse function of any of the registered parse functions.");
                        return new ParseResult(null, 0, false);
                    }
                }

                return f.Parse(text, idx);
            }

            return new ParseResult(null, 0, true);
        }        
    }
}
