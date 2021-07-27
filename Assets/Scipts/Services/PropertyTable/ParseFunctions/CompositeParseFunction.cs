
using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class CompositeParseFunction<TCollection, TElement> : IPolyPropsParseFunction where TCollection : ICollection<TElement>, new()
    {
        public const string DefaultStartToken = "{";
        public const string DefaultEndToken = "}";
        public const string DefaultSeparators = ",";

        public string StartToken { get; set; } = DefaultStartToken;

        public string EndToken { get; set; } = DefaultEndToken;

        public string ElementSeparators { get; set; } = DefaultSeparators;

        public Action<(int, int), string> Log { get; set; }

        public IPolyPropsParseFunction ElementParseFunction { get; set; }

        public Func<string, int, int> SkipWhiteSpaceFunction { get; set; }

        public bool CanParse(string text, int start) => text.IsMatch(StartToken, start, true);

        public ParseResult Parse(string text, int start = 0) => 
            CompositeParseFunction<TCollection, TElement>.Parse(text, ElementParseFunction, SkipWhiteSpaceFunction, start, StartToken, EndToken, ElementSeparators, Log);

        public static ParseResult Parse(
            string text, 
            IPolyPropsParseFunction parseFunction,
            Func<string, int, int> skipWhiteSpaceFunction,
            int start = 0,
            string startToken = DefaultStartToken, 
            string endToken = DefaultEndToken, 
            string separators = DefaultSeparators,
            Action<(int, int), string> log = null)
        {
            if (text.IsMatch(startToken, start, true))
            {
                var idx = start + 1;
                var compositeResult = ParseElements(text, idx, parseFunction, skipWhiteSpaceFunction, endToken, separators);

                idx += compositeResult.charactersRead;

                if (compositeResult.isSuccess && idx < text.Length && text.IsMatch(endToken, idx, true))
                {
                    return new ParseResult(compositeResult.value, (idx - start) + 1, true);
                }

                log?.Invoke(text.GetLineAndColumn(idx), "failed find composite closing delimiter ('" + endToken + "').");
                return new ParseResult(default, start + 1, false);
            }

            log?.Invoke(text.GetLineAndColumn(start), "failed find composite opening delimiter ('" + startToken + "').");
            return new ParseResult(default, start, false);
        }

        public static ParseResult ParseElements(
            string text,
            int start,
            IPolyPropsParseFunction parseFunction,
            Func<string, int, int> skipWhiteSpaceFunction,
            string endToken = "}",
            string separators = ",",
            Action<(int, int), string> log = null
        ) 
        {
            var resultCollection = new TCollection();
            var idx = start;

            while (idx >= 0 && idx < text.Length && !text.IsMatch(endToken, idx, true))
            {
                idx = skipWhiteSpaceFunction(text, idx);

                if (idx < text.Length && !text.IsMatch(endToken, idx, true))
                {
                    var contentResult = parseFunction.Parse(text, idx);

                    if (contentResult.isSuccess)
                    {
                        idx += contentResult.charactersRead;
                        idx = skipWhiteSpaceFunction(text, idx);

                        if (idx < text.Length && !text.IsMatch(endToken, idx, true))
                        {
                            if (separators.IndexOf(text[idx]) < 0)
                            {
                                log?.Invoke(text.GetLineAndColumn(start), "missing separator ('" + separators
                                    + "') after list element ('" + contentResult.value + "').");
                                return new ParseResult(null, idx - start, false);
                            }
                            // skip separator 
                            idx++;
                        }

                        resultCollection.Add((TElement)contentResult.value);
                    }
                    else
                    {
                        return contentResult;
                    }
                }
            }

            return new ParseResult(resultCollection, idx - start, true);
        }
    }
}
