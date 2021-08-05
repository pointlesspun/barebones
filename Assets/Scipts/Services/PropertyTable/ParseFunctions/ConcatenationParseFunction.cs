
using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class ConcatenationParseFunction : AbstractParseFunction
    {

        public List<IParseFunction> Functions { get; set; } = new List<IParseFunction>();

        public override bool CanParse(string text, int start) => Functions.Count > 0 && Functions[0].CanParse(text, start);

        public ParseOperation SkipWhiteSpaceFunction { get; set; }

        public bool AddNullElements { get; set; } = true;

        public bool CompressResult { get; set; } = false;

        public override ParseResult Parse(string text, int start = 0) 
            => ConcatenationParseFunction.Parse(text, start, Functions, SkipWhiteSpaceFunction, AddNullElements, Log, Name, CompressResult);


        public ConcatenationParseFunction Add(params IParseFunction[] functions)
        {
            Functions.AddRange(functions);
            return this;
        }

        public static ParseResult Parse(
            string text, 
            int start, 
            List<IParseFunction> functions, 
            ParseOperation skipFunction, 
            bool addNullElements = true,
            Action<(int, int), string>  log = null,
            string name = "", 
            bool compressResult = false)
        {
            if (functions.Count > 0)
            {
                var result = new List<object>();
                var idx = start;
                var functionIdx = 0;
                var iteration = 0;

                while (idx < text.Length && functionIdx < functions.Count)
                {
                    if (iteration > 100)
                    {
                        throw new InvalidProgramException("ConcatenationParseFunction got into an loop which took too long...");
                    }
                    else 
                    {
                        iteration++;
                    }

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
                                if (functionResult.value != null || addNullElements)
                                {
                                    result.Add(functionResult.value);
                                }
                                    
                                functionIdx++;
                            }
                            else
                            {
                                return functionResult;
                            }
                        }
                        else
                        {
                            log?.Invoke(text.GetLineAndColumn(start), name + " failed to parse function "  + current + "(" + functionIdx + ").");
                            return new ParseResult(null, idx - start, false);
                        }
                    }
                }

                return functionIdx >= functions.Count 
                    ? MapResult(result, idx - start, compressResult)
                    : new ParseResult(null, idx - start, false);
            }
            else
            {
                return new ParseResult(null, 0, true);
            }
        }

        private static ParseResult MapResult(List<object> values, int charactersRead, bool compressResult)
        {
            if (compressResult)
            {
                if (values.Count == 0)
                {
                    return new ParseResult(null, charactersRead, true);
                }
                else if (values.Count == 1)
                {
                    return new ParseResult(values[0], charactersRead, true);
                }

                return new ParseResult(values, charactersRead, true);

            }
            else
            {
                return new ParseResult(values, charactersRead, true);
            }
        }
    }
}
