
using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class ConcatenationParseFunction : IParseFunction
    {
        public Action<(int, int), string> Log { get; set; }

        public List<IParseFunction> Functions { get; set; } = new List<IParseFunction>();

        public bool CanParse(string text, int start) => Functions.Count > 0 && Functions[0].CanParse(text, start);

        public ParseOperation SkipWhiteSpaceFunction { get; set; }

        public ParseResult Parse(string text, int start) 
            => ConcatenationParseFunction.Parse(text, start, Functions, SkipWhiteSpaceFunction);

        public ConcatenationParseFunction Add(params IParseFunction[] functions)
        {
            Functions.AddRange(functions);
            return this;
        }

        public static ParseResult Parse(string text, int start, List<IParseFunction> functions, ParseOperation skipFunction)
        {
            if (functions.Count > 0)
            {
                var result = new List<object>();
                var idx = start;
                var functionIdx = 0;

                while (idx < text.Length && functionIdx < functions.Count)
                {
                    idx = skipFunction(text, idx);

                    if (idx < text.Length)
                    {
                        var current = functions[functionIdx];
                        if (current.CanParse(text, idx))
                        {
                            var functionResult = current.Parse(text, idx);

                            if (functionResult.isSuccess)
                            {
                                idx += functionResult.charactersRead;
                                result.Add(functionResult.value);
                                functionIdx++;
                            }
                            else
                            {
                                return functionResult;
                            }
                        }
                    }
                }

                return functionIdx >= functions.Count 
                    ? new ParseResult(result, start - idx, true)
                    : new ParseResult(null, start - idx, false);
            }
            else
            {
                return new ParseResult(null, 0, true);
            }
        }
    }
}
