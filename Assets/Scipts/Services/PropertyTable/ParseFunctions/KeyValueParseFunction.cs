
using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class KeyValueParseFunction<TKey, TValue> : IPolyPropsParseFunction
    {
        public Action<(int, int), string> Log { get; set; }

        public string SeparatorToken { get; set; } = ":";

        public IPolyPropsParseFunction KeyParseFunction { get; set; }

        public IPolyPropsParseFunction ValueParseFunction { get; set; }

        public Func<string, int, int> SkipWhiteSpaceFunction { get; set; }

        public bool CanParse(string text, int start) => KeyParseFunction.CanParse(text, start);

        public ParseResult Parse(string text, int start) => 
            KeyValueParseFunction<TKey, TValue>.Parse(text, start, KeyParseFunction, ValueParseFunction, SkipWhiteSpaceFunction, SeparatorToken, Log);

        public static ParseResult Parse(
            string text, 
            int start, 
            IPolyPropsParseFunction keyFunction, 
            IPolyPropsParseFunction valueFunction,
            Func<string, int, int> skipFunction,
            string separatorToken = ":",
            Action<(int, int), string> log = null)
        {
            var idx = start;

            var keyResult = keyFunction.Parse(text, idx);

            if (keyResult.isSuccess)
            {
                idx += keyResult.charactersRead;
                idx = skipFunction(text, idx );

                if (idx < text.Length && text.IsMatch(separatorToken, idx))
                {
                    idx += separatorToken.Length;
                    idx = skipFunction(text, idx);

                    if (idx < text.Length)
                    {
                        var valueResult = valueFunction.Parse(text, idx);

                        if (valueResult.isSuccess)
                        {
                            return new ParseResult(
                                new KeyValuePair<TKey, TValue>((TKey)keyResult.value, (TValue)valueResult.value), 
                                (idx +valueResult.charactersRead) - start, 
                                true
                            );
                        }
                        return valueResult;
                    }
                    else
                    {
                        // no value found, assume the key is empty
                        return new ParseResult(new KeyValuePair<TKey, TValue>((TKey)keyResult.value, default(TValue)), idx - start, true);
                    }
                }
                else
                {
                    log?.Invoke(text.GetLineAndColumn(start), "expecting a key/value separator ('" + separatorToken + "') but none was found.");
                    return new ParseResult(null, idx - start, false);
                }
            }

            return keyResult;
        }
    }
}
