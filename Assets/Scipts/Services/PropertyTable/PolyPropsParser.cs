using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public static class PolyPropsParser
    {
        public static List<IPolyPropsExtension> ParseFunctions = new List<IPolyPropsExtension>()
        {

        };

        /**
         * Read the text and return a parsed structure
         * @param text input to parse
         * @param config=null configuration used to parse or PolyPropsConfig.Default if config is null  
         * @returns null in case of errors or be a Dictionary(string, object) or a List(object)
         */
        public static object Read(string text, int start = 0, PolyPropsConfig config = null)
        {
            if (!string.IsNullOrEmpty(text))
            {
                config ??= PolyPropsConfig.Default;

                var idx = SkipWhiteSpaceAndComments(text, start, config);

                if (idx >= 0 && idx < text.Length)
                {
                    if (text[idx] == config.ListDelimiters[0])
                    {
                        return ParseList(text, idx, config).value;
                    }
                    else if (text[idx] == config.MapDelimiters[0])
                    {
                        return ParseMap(text, idx, config).value;
                    }
                    else
                    {
                        var result = ParseCompositeElements<Dictionary<string, object>, KeyValuePair<string, object>>(
                            text, idx, config.MapDelimiters[1], ParseKeyValuePair, config);

                        return result.isSuccess ? result.value : null;
                    }
                }
            }

            return null;
        }

        /** 
         * Parse a 'key' of a key-value pair at the given position in the text. The key either starts 
         * with a string delimiter (usually ' or ") and the key is read as a string or otherwise
         * the key is read as an undelimited string until a PolyPropsConfig.UnquotedStringsDelimiters 
         * is encountered.
         */
        public static ParseResult ParseKey(string text, int start, PolyPropsConfig config)
        {
            var keyStringResult = config.StringDelimiters.IndexOf(text[start]) >= 0
                ? ParseString(text, start, config)
                : ParseUndelimitedString(text, start, config);

            if (keyStringResult.isSuccess)
            {
                var idx = SkipWhiteSpaceAndComments(text, start + keyStringResult.charactersRead, config);

                // valid key found
                if (idx < text.Length && config.KeyValueSeparator.IndexOf(text[idx]) >= 0)
                {
                    return new ParseResult(keyStringResult.value, (idx + 1) - start, true);
                }
                // case of no key-value separator found              
                config.Log(text.GetLineAndColumn(idx),
                    "expected to find a key delimiter: '" + config.KeyValueSeparator + "', but none was found.");
                return new ParseResult(String.Empty, idx - start, false);
            }

            return keyStringResult;
        }

        /**
         * Parse a 'value' in the text at the given start position. A value can either be a standard value
         * (boolean, number, list (array), map (object), string, null or a string without delimiters OR
         * one of the extension types specified in the configuration.
         */
        public static ParseResult ParseValue(string text, int start, PolyPropsConfig config)
        {
            var character = text[start];

            // can an extension handle this ?
            if (config.ParseExtensions != null && config.ParseExtensions.CanParse(text, start))
            {
                return config.ParseExtensions.Parse(text, start);
            }
            // try parse booleans
            if (text.IsMatch(config.BooleanTrue, start, true) || text.IsMatch(config.BooleanFalse, start, true))
            {
                return ParseValue(text, start, (str) => bool.Parse(str), config);
            }
            // try to parse a number
            else if (character == '-' || char.IsDigit(character))
            {
                return ParseNumber(text, start, config);
            }
            // try parse a list
            else if (character == config.ListDelimiters[0])
            {
                return ParseList(text, start, config);
            }
            // try parse a structure
            else if (character == config.MapDelimiters[0])
            {
                return ParseMap(text, start, config);
            }
            // try to parse a string
            else if (config.StringDelimiters.IndexOf(character) >= 0)
            {
                return ParseString(text, start, config);
            }
            // check if null was specified
            else if (text.IsMatch(config.NullValue, start, true))
            {
                return new ParseResult(null, config.NullValue.Length, true);
            }
            // does the config allow for unquoted strings ?
            else if (config.UnquotedStringsDelimiters != string.Empty)
            {
                return ParseUndelimitedString(text, start, config);
            }

            config.Log(text.GetLineAndColumn(start),
                "trying to parse a value starting with: " + character + ", however this character does not match the start of a string, bool, number, map or list.");            
            return new ParseResult(null, 0, false); ; 
        }

        /**
         * Parses a map or object, a collection of key/value pairs, delimited by PolyPropsConfig.MapDelimiters 
         * and separated by PolyPropsConfig.CompositeValueSeparator.
         */
        public static ParseResult ParseMap(string text, int start, PolyPropsConfig config)
            => ParseComposite<Dictionary<string, object>, KeyValuePair<string, object>>(text, start, config.MapDelimiters, ParseKeyValuePair, config);

        /**
         * Parses a list of values delimited by PolyPropsConfig.ListDelimiters 
         * and separated by PolyPropsConfig.CompositeValueSeparator.
         */
        public static ParseResult ParseList(string text, int start, PolyPropsConfig config)
            => ParseComposite<List<object>, object>(text, start, config.ListDelimiters, ParseValue, config);

        public static ParseResult ParseNumber(string text, int start, PolyPropsConfig config)
        {
            var idx = SkipUntilEndCommentOrDelimiter(text, start, config.Separators, config);

            if (idx > start)
            {
                var numberString = text.Substring(start, idx - start);

                try
                {
                    return new ParseResult(numberString.ParseNumber(), idx - start, true);
                }
                catch (Exception e)
                {
                    config.Log(text.GetLineAndColumn(start), "failed to parse number. Exception: " + e);
                    return new ParseResult(null, idx - start, true);
                }
            }

            config.Log(text.GetLineAndColumn(start), "trying to parse a number but no more characters found.");
            return new ParseResult(null, start, true);
        }

        public static ParseResult ParseValue<T>(
            string text,
            int start,
            Func<string, T> parseFunction,
            PolyPropsConfig config
        )
        {
            var endOfToken = text.IndexOfAny(config.Separators, start);

            endOfToken = endOfToken >= 0 ? endOfToken : text.Length;

            try
            {
                return new ParseResult(parseFunction(text.Substring(start, endOfToken - start)), endOfToken - start, true);
            }
            catch (Exception e)
            {
                config.Log(text.GetLineAndColumn(start), "failed to parse value. Exception: " + e);
                return new ParseResult(default, endOfToken - start, false);
            }
        }

        public static ParseResult ParseString(string text, int start, PolyPropsConfig config)
        {
            var firstCharacter = text[start];

            if (config.StringDelimiters.IndexOf(firstCharacter) >= 0)
            {
                var scopedStringLength = text.ReadScopedString(start, firstCharacter);

                if (scopedStringLength > 0)
                {
                    return new ParseResult(text.Substring(start + 1, scopedStringLength - 2), scopedStringLength, true);
                }

                config.Log(text.GetLineAndColumn(start), "failed to parse string, syntax may be incorrect.");
                return new ParseResult(String.Empty, scopedStringLength, false);
            }

            config.Log(text.GetLineAndColumn(start), "failed to parse string, missing beginning quotation mark.");
            return new ParseResult(String.Empty, start, false);
        }

        public static ParseResult ParseUndelimitedString(string text, int start, PolyPropsConfig config)
        {
            var idx = SkipUntilEndCommentOrDelimiter(text, start, config.UnquotedStringsDelimiters, config);
            return new ParseResult(text.Substring(start, idx-start).Trim(), idx-start, true);
        }

        private static ParseResult ParseComposite<TCollection, TElement>(
            string text,
            int start,
            string compositeDelimiters,
            Func<string, int, PolyPropsConfig, ParseResult> parseContentFunction,
            PolyPropsConfig config
        ) where TCollection : ICollection<TElement>, new()
        {           
            if (text[start] == compositeDelimiters[0])
            {
                var compositeEnd = compositeDelimiters[1];
                var idx = start + 1;
                var compositeResult = ParseCompositeElements<TCollection, TElement>(text, idx, compositeEnd, parseContentFunction, config);

                idx += compositeResult.charactersRead;

                if (compositeResult.isSuccess && idx < text.Length && text[idx] == compositeEnd)
                {                    
                    return new ParseResult(compositeResult.value, (idx - start) + 1, true);
                }

                config.Log(text.GetLineAndColumn(idx), "failed find composite closing delimiter ('" + compositeDelimiters[1] + "').");
                return new ParseResult(default, start + 1, false); 
            }
            else
            {
                config.Log(text.GetLineAndColumn(start), "failed find composite opening delimiter ('" + compositeDelimiters[0] + "').");
                return new ParseResult(default, start, false);
            }
        }

        private static ParseResult ParseCompositeElements<TCollection, TElement>(
            string text, 
            int start, 
            char compositeEnd,
            Func<string, int, PolyPropsConfig, ParseResult> parseContentFunction,
            PolyPropsConfig config) where TCollection : ICollection<TElement>, new()
        {
            var resultCollection = new TCollection();
            var idx = start;

            while (idx >= 0 && idx < text.Length && text[idx] != compositeEnd)
            {
                idx = SkipWhiteSpaceAndComments(text, idx, config);

                if (text[idx] != compositeEnd)
                {
                    var contentResult = parseContentFunction(text, idx, config);                   

                    if (contentResult.isSuccess)
                    {
                        idx += contentResult.charactersRead;
                        idx = SkipWhiteSpaceAndComments(text, idx, config);

                        if (idx < text.Length && text[idx] != compositeEnd)
                        {
                            if (config.CompositeValueSeparator.IndexOf(text[idx]) < 0)
                            {
                                config.Log(text.GetLineAndColumn(start), "missing separator ('" + config.CompositeValueSeparator
                                    + "') after list element ('" + contentResult.value + "').");
                                return new ParseResult(null, idx, false);
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

            return new ParseResult(resultCollection, idx  - start, true);
        }

        private static int SkipWhiteSpaceAndComments(string text, int start, PolyPropsConfig config)
        {
            start = text.Skip(config.WhiteSpace, start);

            while (start < text.Length && MatchesSingleLineComment(text, start, config))
            {
                start = GetNextLine(text, start);
                start = text.Skip(config.WhiteSpace, start);
            }

            return start;
        }

        private static int SkipUntilEndCommentOrDelimiter(string text, int start, string delimiters, PolyPropsConfig config)
        {
            while ( start < text.Length
                && delimiters.IndexOf(text[start]) == -1
                && !MatchesSingleLineComment(text, start, config))
            {
                start++;
            }

            return start;
        }

        private static int GetNextLine(string text, int start)
        {
            var eoln = text.IndexOfAny("\r\n", start);
            return eoln >= 0 ? eoln + 1 : text.Length;
        }

        private static bool MatchesSingleLineComment(string text, int idx, PolyPropsConfig config)
        =>
            config.SingleLineCommentToken != String.Empty
                    && text[idx] == config.SingleLineCommentToken[0]
                    && text.IsMatch(config.SingleLineCommentToken, idx);

        private static ParseResult ParseKeyValuePair(string text, int start, PolyPropsConfig config)
        {
            var keyResult = ParseKey(text, start, config);

            if (keyResult.isSuccess)
            {
                var idx = start + keyResult.charactersRead;
                idx = SkipWhiteSpaceAndComments(text, idx, config);

                if (idx < text.Length)
                {
                    var valueResult = ParseValue(text, idx, config);

                    if (valueResult.charactersRead >= 0)
                    {
                        return new ParseResult(
                            new KeyValuePair<string, object>((string)keyResult.value, valueResult.value), 
                            (idx + valueResult.charactersRead) - start, 
                            true);
                    }
                    else
                    {
                        config.Log(text.GetLineAndColumn(start), "failed to parse value.");
                        return new ParseResult(default, (idx + valueResult.charactersRead) - start, false);
                    }
                }
                else
                {
                    return new ParseResult(
                        new KeyValuePair<string, object>((string)keyResult.value, null), 
                        text.Length - start, 
                        true); 
                }
            }
            
            return keyResult;
        }       
    }
}
