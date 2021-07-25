using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public static class PolyPropsParser
    {
        private static (T, int) Error<T>() => (default(T), -1);        
       
        public static Dictionary<string, object> Read(
            string text,
            PolyPropsConfig config = null
        )
        {
            config ??= PolyPropsConfig.Default;

            var idx = SkipWhiteSpaceAndComments(text, 0, config);

            if (idx >= 0 && idx < text.Length)
            {
                var result = new Dictionary<string, object>();
                
                return ParseCompositeElements(result, text, idx, config.MapDelimiters[1], ParseKeyValuePair, config) >= 0
                    ? result
                    : null;
            }

            return null;
        }

        public static (string key, int charactersRead) ParseKey(string text, int start, PolyPropsConfig config)
        {
            var (key, charactersRead) = config.StringDelimiters.IndexOf(text[start]) >= 0
                ? ParseString(text, start, config)
                : ParseUndelimitedString(text, start, config);

            var idx = SkipWhiteSpaceAndComments(text, start + charactersRead, config);

            // valid key found
            if (idx < text.Length && text[idx] == config.KeyValueSeparator)
            {
                return (key, (idx + 1) - start);
            }
            // case of no closing column
            else
            {
                config.Log(text.GetLineAndColumn(start), 
                    "expected to find a key delimiter: '" + config.KeyValueSeparator + "', but none was found.");
                return Error<string>();
            }
        }
                        
        public static (object value, int charactersRead) ParseValue(string text, int start, PolyPropsConfig config)
        {
            var character = text[start];

            // try parse booleans
            if ((character == 't' || character == 'f') // quick check before doing the heavier match
                && (text.IsMatch("true", start) || text.IsMatch("false", start)))
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
            else if (config.UnquotedStringsDelimiters != string.Empty)
            {
                return ParseUndelimitedString(text, start, config);
            }

            config.Log(text.GetLineAndColumn(start),
                "trying to parse a value starting with: " + character + ", however this character does not match the start of a string, bool, number, map or list.");
            return Error<object>(); 
        }

        public static (Dictionary<string, object> value, int charactersRead) ParseMap(string text, int idx, PolyPropsConfig config)
            => ParseComposite<Dictionary<string, object>>(text, idx, config.MapDelimiters, ParseKeyValuePair, config);
        

        public static (List<object> arrayValue, int charactersRead) ParseList(string text, int idx, PolyPropsConfig config)
            => ParseComposite<List<object>>(text, idx, config.ListDelimiters, ParseListElement, config);

        public static (object value, int charactersRead) ParseNumber(string text,
            int start,
            PolyPropsConfig config
        )
        {
            var idx = SkipUntilEndCommentOrDelimiter(text, start, config.Separators, config);

            if (idx > start)
            {
                var numberString = text.Substring(start, idx - start);

                try
                {
                    return (numberString.ParseNumber(), idx - start);
                }
                catch (Exception e)
                {
                    config.Log(text.GetLineAndColumn(start), "failed to parse number. Exception: " + e);
                    return Error<int>();
                }
            }

            config.Log(text.GetLineAndColumn(start), "trying to parse a number but no more characters found.");

            return Error<Decimal>();
        }

        public static (T value, int charactersRead) ParseValue<T>(
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
                var value = parseFunction(text.Substring(start, endOfToken - start));
                return (value, endOfToken - start);
            }
            catch (Exception e)
            {
                config.Log(text.GetLineAndColumn(start), "failed to parse value. Exception: " + e);
                return Error<T>();
            }
        }

        public static (string stringValue, int charactersRead) ParseString(string text, int start, PolyPropsConfig config)
        {
            var firstCharacter = text[start];

            if (config.StringDelimiters.IndexOf(firstCharacter) >= 0)
            {
                var scopedStringLength = text.ReadScopedString(start, firstCharacter);

                if (scopedStringLength > 0)
                {
                    return (text.Substring(start + 1, scopedStringLength - 2), scopedStringLength);
                }
                else
                {
                    config.Log(text.GetLineAndColumn(start), "failed to parse string, syntax may be incorrect.");
                }
            }
            else
            {
                config.Log(text.GetLineAndColumn(start), "failed to parse string, missing beginning quotation mark.");
            }

            return Error<string>();
        }

        public static (string stringValue, int charactersRead) ParseUndelimitedString(string text, int start, PolyPropsConfig config)
        {
            var idx = SkipUntilEndCommentOrDelimiter(text, start, config.UnquotedStringsDelimiters, config);
            return (text.Substring(start, idx-start).Trim(), idx-start);
        }

        private static (T value, int charactersRead) ParseComposite<T>(
            string text,
            int start,
            string compositeDelimiters,
            Func<T, string, int, PolyPropsConfig, int> parseContentFunction,
            PolyPropsConfig config
        ) where T : new()
        {           
            if (text[start] == compositeDelimiters[0])
            {
                var compositeEnd = compositeDelimiters[1];
                var result = new T();
                var idx = ParseCompositeElements(result, text, start + 1, compositeEnd, parseContentFunction, config);

                if (idx >= 0 && idx < text.Length && text[idx] == compositeEnd)
                {
                    return (result, (idx - start) + 1);
                }
                else
                {
                    config.Log(text.GetLineAndColumn(idx), "failed find closing delimiter ('" + compositeDelimiters[1] + "').");
                }

                return Error<T>();
            }
            else
            {
                config.Log(text.GetLineAndColumn(start),  "failed find opening delimiter ('" + compositeDelimiters[0] + "').");
                return Error<T>();  
            }
        }

        private static int ParseCompositeElements<T>(
            T result, 
            string text, 
            int idx, 
            char compositeEnd,
            Func<T, string, int, PolyPropsConfig, int> parseContentFunction,
            PolyPropsConfig config) where T : new()
        {
            while (idx >= 0 && idx < text.Length && text[idx] != compositeEnd)
            {
                idx = SkipWhiteSpaceAndComments(text, idx, config);

                if (text[idx] != compositeEnd)
                {
                    idx = parseContentFunction(result, text, idx, config);                   
                }
            }

            return idx;
        }

        private static int SkipWhiteSpaceAndComments(string text, int idx, PolyPropsConfig config)
        {
            idx = text.Skip(config.WhiteSpace, idx);

            while (idx < text.Length && MatchesSingleLineComment(text, idx, config))
            {
                idx = GetNextLine(text, idx);
                idx = text.Skip(config.WhiteSpace, idx);
            }

            return idx;
        }

        private static int SkipUntilEndCommentOrDelimiter(string text, int idx, string delimiters, PolyPropsConfig config)
        {
            while ( idx < text.Length
                && delimiters.IndexOf(text[idx]) == -1
                && !MatchesSingleLineComment(text, idx, config))
            {
                idx++;
            }

            return idx;
        }

        private static int GetNextLine(string text, int index)
        {
            var eoln = text.IndexOfAny("\r\n", index);
            return eoln >= 0 ? eoln + 1 : text.Length;
        }

        private static bool MatchesSingleLineComment(string text, int idx, PolyPropsConfig config)
        =>
            config.SingleLineCommentToken != String.Empty
                    && text[idx] == config.SingleLineCommentToken[0]
                    && text.IsMatch(config.SingleLineCommentToken, idx);

        private static int ParseKeyValuePair(
            Dictionary<string, object> result, 
            string text, 
            int idx,
            PolyPropsConfig config)
        {
            var (key, charactersRead) = ParseKey(text, idx, config);

            if (key != default)
            {
                idx += charactersRead;
                idx = SkipWhiteSpaceAndComments(text, idx, config);

                if (idx < text.Length)
                {
                    var parseResult = ParseValue(text, idx, config);

                    if (parseResult.value != null)
                    {
                        result[key] = parseResult.value;
                        idx += parseResult.charactersRead;
                    }
                    else
                    {
                        config.Log(text.GetLineAndColumn(idx), "failed to parse value.");
                        return -1;
                    }

                    idx = SkipWhiteSpaceAndComments(text, idx, config);

                    if (idx < text.Length && text[idx] != config.MapDelimiters[1])
                    {
                        if (config.CompositeValueSeparator.IndexOf(text[idx]) < 0)
                        {
                            config.Log(text.GetLineAndColumn(idx), "missing separator.");
                            return -1;
                        }

                        idx++;
                    }
                }
                else
                {
                    result[key] = null;
                }
            }
            else
            {
                config.Log(text.GetLineAndColumn(idx), "cannot parse key.");
                return -1;
            }

            return idx;
        }       

        private static int ParseListElement(
            List<object> result,
            string text,
            int idx,
            PolyPropsConfig config
        )
        {
            var (value, charactersRead) = ParseValue(text, idx, config);

            if (value != null)
            {
                result.Add(value);
                idx += charactersRead;
            }
            else
            {
                config.Log(text.GetLineAndColumn(idx), "failed to list value.");
                return -1;
            }

            idx = SkipWhiteSpaceAndComments(text, idx, config);

            if (idx < text.Length && text[idx] != config.ListDelimiters[1])
            {
                if (config.CompositeValueSeparator.IndexOf(text[idx]) < 0)
                {
                    config.Log(text.GetLineAndColumn(idx),  "missing separator.");
                    return -1;
                }

                idx++;
            }

            return idx;
        }
    }
}
