using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class PolyPropsConfig
    {
        public static readonly PolyPropsConfig Default = new PolyPropsConfig();

        public string WhiteSpace { get; set; } = " \t\r\n";

        public string Separators { get; set; } = " \t\n\r,[]{}";

        public string MapDelimiters { get; set; } = "{}";

        public string ListDelimiters { get; set; } = "[]";

        public string StringDelimiters { get; set; } = "\"'";    

        public char KeyValueSeparator { get; set; } = ':';

        public string CompositeValueSeparator { get; set; } = ",";
    }

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
                
                return ParseCompositeElements(result, text, idx, config.MapDelimiters[1], ParseStructureContent, config) >= 0
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
                Debug.LogWarning("PropertyTableParser.ParseKey, key not delimited with a separator ('" + config.KeyValueSeparator + "').");
                return Error<string>();
            }
        }
                        
        public static (object value, int charactersRead) ParseValue(
            string text,
            int start,
            PolyPropsConfig config
        )
        {
            var valueStartIdx = text.Skip(config.WhiteSpace, start);

            // end-of-line or end-of-file?
            if (valueStartIdx == -1 || "\n\r".IndexOf(text[valueStartIdx]) >= 0)
            {
                return Error<object>(); 
            }

            var whiteCharacterCount = valueStartIdx - start;
            var character = text[valueStartIdx];

            // try parse booleans
            if ((character == 't' || character == 'f') // quick check before doing the heavier match
                && (ParseUtil.MatchLength(text, "true", valueStartIdx) == 4
                || ParseUtil.MatchLength(text, "false", valueStartIdx) == 5))
            {
                var (booleanValue, charactersRead) = 
                    ParsePODValue(text, valueStartIdx, (str) => bool.Parse(str), config.Separators);
                return (booleanValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a number
            else if (character == '-' || char.IsDigit(character))
            {
                var (numberValue, charactersRead) = 
                    ParsePODValue(text, valueStartIdx, (str) => float.Parse(str), config.Separators);
                return (numberValue, charactersRead + whiteCharacterCount);
            }
            // try parse a list
            else if (character == config.ListDelimiters[0])
            {
                var (arrayValue, charactersRead) = ParseListValue(text, valueStartIdx, config);
                return (arrayValue, charactersRead + whiteCharacterCount);
            }
            // try parse a structure
            else if (character == config.MapDelimiters[0])
            {
                var (structValue, charactersRead) = ParseStructureValue(text, valueStartIdx, config);
                return (structValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a string
            else  if (config.StringDelimiters.IndexOf(character) >= 0)
            {
                var (numberValue, charactersRead) = ParseStringValue(text, valueStartIdx, config);
                return (numberValue, charactersRead + whiteCharacterCount);
            }

            Debug.LogWarning("PropertyTableParser.ParseValue, unexpected value: " + character + ", expected string, bool, float, array or struct.");
            return Error<object>(); 
        }

        public static (Dictionary<string, object> value, int charactersRead) ParseStructureValue(
            string text,
            int start,
            PolyPropsConfig config
        )
            => ParseComposite<Dictionary<string, object>>(text, start, config.MapDelimiters, ParseStructureContent, config);
        

        public static (List<object> arrayValue, int charactersRead) ParseListValue(
            string text,
            int start,
            PolyPropsConfig config
        )
        => ParseComposite<List<object>>(text, start, config.ListDelimiters, ParseListContent, config);


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
                    Debug.LogWarning("PropertyTableParser.ParseComposite, failed find closing delimiter ('" + compositeDelimiters[1] + "').");
                }

                return Error<T>();
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find opening delimiter ('" + compositeDelimiters[0] + "').");
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

        private static int ParseStructureContent(
            Dictionary<string, object> result, 
            string text, 
            int idx,
            PolyPropsConfig config)
        {
            var (key, charactersRead) = ParseKey(text, idx, config);

            if (key != default)
            {
                idx += charactersRead;
                var parseResult = ParseValue(text, idx, config);

                if (parseResult.value != null)
                {
                    result[key] = parseResult.value;
                    idx += parseResult.charactersRead;
                }
                else
                {
                    Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed to parse value.");
                    return -1;
                }               

                idx = text.Skip(config.WhiteSpace, idx);

                if (idx < text.Length && text[idx] != '}')
                {
                    if (config.CompositeValueSeparator.IndexOf(text[idx]) < 0)
                    {
                        Debug.LogWarning("PropertyTableParser.ParseArrayValue, missing separator.");
                        return -1;
                    }

                    idx++;
                }
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseStructureValue, key is not valid.");
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
            var parseResult = ParseValue(text, idx, config);

            if (parseResult.value != null)
            {
                result.Add(parseResult.value);
                idx += parseResult.charactersRead;
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed to parse value.");
                return -1;
            }

            // skip white space
            idx = text.Skip(config.WhiteSpace, idx); 

            if (idx < text.Length && text[idx] != config.ListDelimiters[1])
            {
                if (config.CompositeValueSeparator.IndexOf(text[idx]) < 0)
                {
                    Debug.LogWarning("PropertyTableParser.ParseListContent, missing separator.");
                    return -1;
                }

                idx++;
            }

            return idx;
        }

        public static (T value, int charactersRead) ParsePODValue<T>(
            string text,
            int start,
            Func<string, T> parseFunction,
            string endOfTokenCharacters = " \t\r\n"
        )
        {
            var endOfToken = text.IndexOfAny(endOfTokenCharacters, start);

            endOfToken = endOfToken >= 0 ? endOfToken : text.Length;

            try
            {
                var value = parseFunction(text.Substring(start, endOfToken - start));
                return (value, endOfToken - start);
            }
            catch (Exception e)
            {
                Debug.LogWarning("PropertyTableParser.ParsePODValue, failed to parse value. Exception: " + e);
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
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseStringValue: failed to parse string, missing beginning quotation mark.");
            }

            return Error<string>();
        }
    }
}
