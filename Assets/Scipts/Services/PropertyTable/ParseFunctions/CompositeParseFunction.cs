
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class CompositeParseFunction<TCollection, TElement> : AbstractParseFunction 
        where TCollection : ICollection<TElement>, new()
    {
        public const string DefaultStartToken = "{";
        public const string DefaultEndToken = "}";
        public const string DefaultSeparators = ",";

        public string StartToken { get; set; } = DefaultStartToken;

        public string EndToken { get; set; } = DefaultEndToken;

        public string ElementSeparators { get; set; } = DefaultSeparators;

        public bool ContinueAfterError { get; set; } = true;

        public IParseFunction ElementParseFunction { get; set; }

        public ParseOperation SkipWhiteSpaceFunction { get; set; }

        public override bool CanParse(string text, int start) => text.IsMatch(StartToken, start, true);

        public override ParseResult Parse(string text, int start = 0) => 
            CompositeParseFunction<TCollection, TElement>.Parse(
                text, ElementParseFunction, SkipWhiteSpaceFunction, start, StartToken, EndToken, ElementSeparators, ContinueAfterError, Log);

        public static ParseResult Parse(
            string text, 
            IParseFunction parseFunction,
            ParseOperation skipWhiteSpaceFunction,
            int start = 0,
            string startToken = DefaultStartToken, 
            string endToken = DefaultEndToken, 
            string separators = DefaultSeparators,
            bool continueAfterError = true,
            Action<(int, int), string> log = null)
        {
            Debug.Assert(parseFunction != null, "CompositeParseFunction: no element parse function defined.");
            Debug.Assert(skipWhiteSpaceFunction != null, "CompositeParseFunction: no skip white space function defined.");

            if (string.IsNullOrEmpty(startToken) || text.IsMatch(startToken, start, true))
            {
                var idx = start + (string.IsNullOrEmpty(startToken) ? 0 : 1);
                var compositeResult = ParseElements(text, idx, parseFunction, skipWhiteSpaceFunction, endToken, separators, continueAfterError, log);

                idx += compositeResult.charactersRead;

                if (compositeResult.isSuccess)
                {
                    if (string.IsNullOrEmpty(endToken) || (idx < text.Length && (text.IsMatch(endToken, idx, true))))
                    { 
                        var charactersRead = (idx - start) + (string.IsNullOrEmpty(endToken) ? 0 : 1);
                        return new ParseResult(compositeResult.value, charactersRead, true);
                    }
                    log?.Invoke(text.GetLineAndColumn(idx), "failed find composite closing delimiter ('" + endToken + "').");
                    return new ParseResult(default, start + 1, false);
                }

                return compositeResult;
            }

            log?.Invoke(text.GetLineAndColumn(start), "failed find composite opening delimiter ('" + startToken + "').");
            return new ParseResult(default, start, false);
        }

        // xxx the complexity is way too high
        public static ParseResult ParseElements(
            string text,
            int start,
            IParseFunction parseFunction,
            ParseOperation skipWhiteSpaceFunction,
            string endToken = "}",
            string separators = ",",
            bool continueAfterError = true,
            Action<(int, int), string> log = null
        ) 
        {
            Debug.Assert(parseFunction != null, "Cannot parse elements with null ParseFunction.");
            Debug.Assert(skipWhiteSpaceFunction != null, "Cannot parse elements with null skip ws function.");

            var resultCollection = new TCollection();
            var idx = start;
            var noErrorsEncountered = true;
            var iteration = 0;

            while (idx >= 0 && idx < text.Length && (string.IsNullOrEmpty(endToken) || !text.IsMatch(endToken, idx, true)))
            {
                if (iteration > 100)
                {
                    throw new InvalidProgramException("CompositeParseFunction got into an loop which took too long...");
                }
                else
                {
                    iteration++;
                }

                idx = skipWhiteSpaceFunction(text, idx);

                if (idx < text.Length && (string.IsNullOrEmpty(endToken) || !text.IsMatch(endToken, idx, true)))
                {
                    var contentResult = parseFunction.Parse(text, idx);

                    if (contentResult.isSuccess)
                    {
                        idx += contentResult.charactersRead;
                        idx = skipWhiteSpaceFunction(text, idx);

                        if (idx < text.Length && (string.IsNullOrEmpty(endToken) || !text.IsMatch(endToken, idx, true)))
                        {
                            if (separators.IndexOf(text[idx]) < 0)
                            {
                                log?.Invoke(text.GetLineAndColumn(start), "missing separator ('" + separators
                                    + "') after list element ('" + contentResult.value + "').");

                                // attempt to recover ? 
                                if (continueAfterError)
                                {
                                    noErrorsEncountered = false;

                                    idx = ParseUtil.SkipUntil(text, idx + contentResult.charactersRead,
                                        (txt, i) => separators.IndexOf(txt[i]) >= 0 || ParseUtil.IsMatch(txt, endToken, i));

                                    if (idx >= text.Length)
                                    {
                                        return contentResult;
                                    }
                                    // else try parse the next element
                                    idx++;
                                }
                                else
                                {
                                    return new ParseResult(null, idx - start, false);
                                }
                            }
                            else
                            {
                                // skip separator 
                                idx++;
                            }
                        }

                        resultCollection.Add((TElement)contentResult.value);
                    }
                    // attempt to recover ?
                    else if (continueAfterError)
                    {
                        noErrorsEncountered = false;

                        idx = ParseUtil.SkipUntil(text, idx + contentResult.charactersRead,
                            (txt, i) => separators.IndexOf(txt[i]) >= 0 || ParseUtil.IsMatch(txt, endToken, i));

                        if (idx >= text.Length)
                        {
                            return contentResult;
                        }
                        // else try parse the next element
                        idx++;
                    }
                    else
                    {
                        return contentResult;
                    }
                }
            }

            return new ParseResult(resultCollection, idx - start, noErrorsEncountered);
        }
    }
}
