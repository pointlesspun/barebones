
using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class RepeatParseFunction<TCollection, TElement> : AbstractParseFunction where TCollection : ICollection<TElement>, new()
    {
        public IParseFunction Function { get; set; }

        public int Min { get; set; } = -1;

        public int Max { get; set; } = -1;

        public override bool CanParse(string text, int start) => Function != null && Function.CanParse(text, start);

        public ParseOperation SkipWhiteSpaceOperation { get; set; }

        public ParseOperation TerminationOperation { get; set; }

        public ParseOperation SeparationOperation { get; set; }

        public bool ContinueAfterError { get; set; } = true;

        public bool ConsumeTermination { get; set; } = true;

        public bool AddNullElements { get; set; } = true;
 
        public override ParseResult Parse(string text, int start = 0)
        {
            return Parse(text, Function, SkipWhiteSpaceOperation, TerminationOperation, SeparationOperation, start, ContinueAfterError, Min, Max, ConsumeTermination, AddNullElements);
        }

        
        public static ParseResult Parse(
            string text, 
            IParseFunction elementFunction, 
            ParseOperation skipWhiteSpaceFunction,
            ParseOperation terminationFunction,
            ParseOperation separatorFunction = null,
            int start = 0, 
            bool continueAfterErrors = true, 
            int min = -1, 
            int max = -1,
            bool consumeTerminationCharacters = true,
            bool addNullElements = true
        )
        {
            var idx = start;
            var result = new TCollection();
            var noErrorsEncountered = true;

            if (!String.IsNullOrEmpty(text))
            {
                // guard
                if (terminationFunction != null)
                {
                    var count = terminationFunction(text, idx);

                    if (count >= 0)
                    {
                        return consumeTerminationCharacters
                            ? new ParseResult(result, count - start, true)
                            : new ParseResult(result, 0, true);
                    }
                }

                var iteration = 0;

                while ((terminationFunction == null || terminationFunction(text, idx) == -1) 
                    && idx < text.Length
                    && elementFunction.CanParse(text, idx)
                    && (max == -1 || result.Count < max))
                {
                    if (iteration > 100)
                    {
                        throw new InvalidProgramException("RepeatParseFunction got into an loop which took too long...");
                    }
                    else
                    {
                        iteration++;
                    }

                    var functionResult = elementFunction.Parse(text, idx);

                    // parse an element
                    if (functionResult.isSuccess)
                    {
                        if (functionResult.value != null || addNullElements)
                        {
                            result.Add((TElement)functionResult.value);
                        }

                        idx += functionResult.charactersRead;
                    }
                    else if (!CanContinueAfterError(idx + functionResult.charactersRead))
                    {
                        return functionResult;
                    }

                    // check the next character/symbol after the whitespace
                    idx = skipWhiteSpaceFunction(text, idx);

                    // more text available ?
                    if (idx < text.Length)
                    {
                        // end found, breakt to go to the check of the termination conditions (min, max)
                        if (terminationFunction != null)
                        {
                            var terminationCharactersRead = terminationFunction(text, idx);

                            if (terminationCharactersRead >= 0)
                            {
                                idx = consumeTerminationCharacters ? terminationCharactersRead : idx;

                                return (min < 0 || result.Count >= min)
                                        ? new ParseResult(result, idx - start, noErrorsEncountered)
                                        : new ParseResult(null, idx - start, false);
                            }
                        }

                        // more characters found, must be separators if any are defined
                        if (separatorFunction != null)
                        {
                            var nextIdx = separatorFunction(text, idx);

                            if (nextIdx >= idx)
                            {
                                // separator found - skip the next whitespace
                                idx = skipWhiteSpaceFunction(text, nextIdx + 1);
                            }
                            // no sepatator encountered 
                            else if (CanContinueAfterError(idx))
                            {
                                idx = skipWhiteSpaceFunction(text, idx);
                            }
                            else
                            {
                                return new ParseResult(null, idx - start, false);
                            }
                        }
                    }
                }
            }

            // the terminationFunction must have been met (if defined)
            return terminationFunction == null    
                    ? new ParseResult(result, idx - start, noErrorsEncountered)
                    : new ParseResult(null, idx - start, false);

            bool CanContinueAfterError(int position)
            {
                if (continueAfterErrors && separatorFunction != null)
                {
                    noErrorsEncountered = false;

                    idx = ParseUtil.SkipUntil(text, position, (txt, i) => separatorFunction(txt, i) >= 0);

                    // else try parse the next element
                    idx++;

                    return idx < text.Length;
                }
                else
                {
                    return false;                
                }
            }
        }
    }
}
