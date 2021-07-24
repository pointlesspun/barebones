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

            var idx = text.Skip(config.WhiteSpace, 0);

            if (idx >= 0 && idx < text.Length)
            {
                var result = new Dictionary<string, object>();
                
                return ParseCompositeElements(result, text, idx, config.MapDelimiters[1], ParseMapContent, config) >= 0
                    ? result
                    : null;
            }

            return null;
        }

        public static (string key, int charactersRead) ParseKey(string text, int start, PolyPropsConfig config)
        {
            var endOfKey = text.IndexOf(config.KeyValueSeparator, start);

            // valid key found
            if (endOfKey >= 0)
            {
                return (text.Substring(start, endOfKey - start).Trim(), (endOfKey - start) + 1);
            }
            // case of no closing column
            else
            {
                config.Log(text.GetLineAndColumn(start), 
                    "expected to find a key delimiter: '" + config.KeyValueSeparator + "', but none was found.");
                return Error<string>();
            }
        }
                        
        public static (object value, int charactersRead) ParseValue(
            string text,
            int start,
            PolyPropsConfig config
        )
        {
            var character = text[start];

            // try parse booleans
            if ((character == 't' || character == 'f') // quick check before doing the heavier match
                && (ParseUtil.MatchLength(text, "true", start) == 4
                || ParseUtil.MatchLength(text, "false", start) == 5))
            {
                var (booleanValue, charactersRead) = 
                    ParsePODValue(text, start, (str) => bool.Parse(str), config);
                return (booleanValue, charactersRead);
            }
            // try to parse a number
            else if (character == '-' || char.IsDigit(character))
            {
                var (numberValue, charactersRead) = 
                    ParsePODValue(text, start, (str) => float.Parse(str), config);
                return (numberValue, charactersRead);
            }
            // try parse a list
            else if (character == config.ListDelimiters[0])
            {
                var (arrayValue, charactersRead) = ParseListValue(text, start, config);
                return (arrayValue, charactersRead);
            }
            // try parse a structure
            else if (character == config.MapDelimiters[0])
            {
                var (structValue, charactersRead) = ParseStructureValue(text, start, config);
                return (structValue, charactersRead);
            }
            // try to parse a string
            else  if (config.StringDelimiters.IndexOf(character) >= 0)
            {
                var (numberValue, charactersRead) = ParseStringValue(text, start, config);
                return (numberValue, charactersRead);
            }

            config.Log(text.GetLineAndColumn(start),
                "trying to parse a value starting with: " + character + ", however this character does not match the start of a string, bool, number, map or list.");
            return Error<object>(); 
        }

        public static (Dictionary<string, object> value, int charactersRead) ParseStructureValue(
            string text,
            int start,
            PolyPropsConfig config
        )
            => ParseComposite<Dictionary<string, object>>(text, start, config.MapDelimiters, ParseMapContent, config);
        

        public static (List<object> arrayValue, int charactersRead) ParseListValue(
            string text,
            int start,
            PolyPropsConfig config
        )
        => ParseComposite<List<object>>(text, start, config.ListDelimiters, ParseListContent, config);

        public static (T value, int charactersRead) ParsePODValue<T>(
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

        public static (string stringValue, int charactersRead) ParseStringValue(string text, int start, PolyPropsConfig config)
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
                idx = text.Skip(config.WhiteSpace, idx);

                if (text[idx] != compositeEnd)
                {
                    idx = parseContentFunction(result, text, idx, config);                   
                }
            }

            return idx;
        }

        private static int ParseMapContent(
            Dictionary<string, object> result, 
            string text, 
            int idx,
            PolyPropsConfig config)
        {
            var (key, charactersRead) = ParseKey(text, idx, config);

            if (key != default)
            {
                idx += charactersRead;
                idx = text.Skip(config.WhiteSpace, idx);

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

                    idx = text.Skip(config.WhiteSpace, idx);

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


        private static int ParseListContent(
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

            // skip white space
            idx = text.Skip(config.WhiteSpace, idx);

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
