using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public static class PolyPropsParser
    {
        /** Code returned when the parser encounters an error in some cases*/
        public static (T, int) Error<T>() => (default(T), -1);

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
                        var result = new Dictionary<string, object>();

                        return ParseCompositeElements(result, text, idx, config.MapDelimiters[1], ParseKeyValuePair, config) >= 0
                            ? result
                            : null;
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
        public static (string key, int charactersRead) ParseKey(string text, int start, PolyPropsConfig config)
        {
            var (key, charactersRead) = config.StringDelimiters.IndexOf(text[start]) >= 0
                ? ParseString(text, start, config)
                : ParseUndelimitedString(text, start, config);

            var idx = SkipWhiteSpaceAndComments(text, start + charactersRead, config);

            // valid key found
            if (idx < text.Length && config.KeyValueSeparator.IndexOf(text[idx]) >= 0)
            {
                return (key, (idx + 1) - start);
            }
            // case of no key-value separator found
            else
            {
                config.Log(text.GetLineAndColumn(idx), 
                    "expected to find a key delimiter: '" + config.KeyValueSeparator + "', but none was found.");
                return Error<string>();
            }
        }

        /**
         * Parse a 'value' in the text at the given start position. A value can either be a standard value
         * (boolean, number, list (array), map (object), string, null or a string without delimiters OR
         * one of the extension types specified in the configuration.
         */
        public static (object value, int charactersRead) ParseValue(string text, int start, PolyPropsConfig config)
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
                return (null, config.NullValue.Length);
            }
            // does the config allow for unquoted strings ?
            else if (config.UnquotedStringsDelimiters != string.Empty)
            {
                return ParseUndelimitedString(text, start, config);
            }

            config.Log(text.GetLineAndColumn(start),
                "trying to parse a value starting with: " + character + ", however this character does not match the start of a string, bool, number, map or list.");
            return Error<object>(); 
        }

        public static (Dictionary<string, object> value, int charactersRead) ParseMap(string text, int start, PolyPropsConfig config)
            => ParseComposite<Dictionary<string, object>>(text, start, config.MapDelimiters, ParseKeyValuePair, config);
        

        public static (List<object> value, int charactersRead) ParseList(string text, int start, PolyPropsConfig config)
            => ParseComposite<List<object>>(text, start, config.ListDelimiters, ParseListElement, config);

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
            int start, 
            char compositeEnd,
            Func<T, string, int, PolyPropsConfig, int> parseContentFunction,
            PolyPropsConfig config) where T : new()
        {
            while (start >= 0 && start < text.Length && text[start] != compositeEnd)
            {
                start = SkipWhiteSpaceAndComments(text, start, config);

                if (text[start] != compositeEnd)
                {
                    start = parseContentFunction(result, text, start, config);                   
                }
            }

            return start;
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

        private static int ParseKeyValuePair(
            Dictionary<string, object> result, 
            string text, 
            int start,
            PolyPropsConfig config)
        {
            var (key, charactersRead) = ParseKey(text, start, config);

            if (charactersRead > 0)
            {
                start += charactersRead;
                start = SkipWhiteSpaceAndComments(text, start, config);

                if (start < text.Length)
                {
                    var parseResult = ParseValue(text, start, config);

                    if (parseResult.charactersRead >= 0)
                    {
                        result[key] = parseResult.value;
                        start += parseResult.charactersRead;
                    }
                    else
                    {
                        config.Log(text.GetLineAndColumn(start), "failed to parse value.");
                        return -1;
                    }

                    start = SkipWhiteSpaceAndComments(text, start, config);

                    if (start < text.Length && text[start] != config.MapDelimiters[1])
                    {
                        if (config.CompositeValueSeparator.IndexOf(text[start]) < 0)
                        {
                            config.Log(text.GetLineAndColumn(start), "missing separator.");
                            return -1;
                        }

                        start++;
                    }
                }
                else
                {
                    result[key] = null;
                }
            }
            else
            {
                config.Log(text.GetLineAndColumn(start), "cannot parse key.");
                return -1;
            }

            return start;
        }       

        private static int ParseListElement(
            List<object> result,
            string text,
            int start,
            PolyPropsConfig config
        )
        {
            var (value, charactersRead) = ParseValue(text, start, config);

            if (charactersRead >= 0)
            {
                result.Add(value);
                start += charactersRead;
            }
            else
            {
                config.Log(text.GetLineAndColumn(start), "failed to list value.");
                return -1;
            }

            start = SkipWhiteSpaceAndComments(text, start, config);

            if (start < text.Length && text[start] != config.ListDelimiters[1])
            {
                if (config.CompositeValueSeparator.IndexOf(text[start]) < 0)
                {
                    config.Log(text.GetLineAndColumn(start),  "missing separator.");
                    return -1;
                }

                start++;
            }

            return start;
        }
    }
}
