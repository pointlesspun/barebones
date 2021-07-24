using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class PolyPropsConfig
    {
        public string WhiteSpace { get; set; } = " \t\r\n";
        public string Separators { get; set; } = " \t\n\r,[]{}";
    }

    public static class PolyPropsParser
    {
        private static (T, int) Error<T>() => (default(T), -1);        
       
        public static Dictionary<string, object> Read(
            string text, 
            string whiteSpace = " \t\r\n",
            string separators = " \t\n\r,[]{}"
        )
        {
            var idx = text.Skip(whiteSpace, 0);

            if (idx >= 0 && idx < text.Length)
            {
                var result = new Dictionary<string, object>();
                
                return ParseCompositeElements(result, text, idx, (char) 0, ParseStructureContent, whiteSpace, separators) >= 0
                    ? result
                    : null;
            }

            return null;
        }

        public static (string key, int charactersRead) ParseKey(string text, int start, int line)
        {
            var endOfKey = text.IndexOf(':', start);

            // valid key found
            if (endOfKey >= 0)
            {
                return (text.Substring(start, endOfKey - start).Trim(), (endOfKey - start) + 1);
            }
            // case of no closing column
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseKey, key not delimited with a column ':' at line " + line + ".");
                return Error<string>();
            }
        }
                        
        public static (object value, int charactersRead) ParseValue(
            string text,
            int start,
            string whiteSpace = " \t",
            string separators = " \t\n\r,[]{}"
        )
        {
            var valueStartIdx = text.Skip(whiteSpace, start);

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
                    ParsePODValue(text, valueStartIdx, (str) => bool.Parse(str), separators);
                return (booleanValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a number
            else if (character == '-' || char.IsDigit(character))
            {
                var (numberValue, charactersRead) = 
                    ParsePODValue(text, valueStartIdx, (str) => float.Parse(str), separators);
                return (numberValue, charactersRead + whiteCharacterCount);
            }
            // try parse an array
            else if (character == '[')
            {
                var (arrayValue, charactersRead) = ParseListValue(text, valueStartIdx);
                return (arrayValue, charactersRead + whiteCharacterCount);
            }
            // try parse a structure
            else if (character == '{')
            {
                var (structValue, charactersRead) = ParseStructureValue(text, valueStartIdx);
                return (structValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a string
            else  if (character == '"' || character == '\'')
            {
                var (numberValue, charactersRead) = ParseStringValue(text, valueStartIdx);
                return (numberValue, charactersRead + whiteCharacterCount);
            }

            Debug.LogWarning("PropertyTableParser.ParseValue, unexpected value: " + character + ", expected string, bool, float, array or struct.");
            return Error<object>(); 
        }

        public static (Dictionary<string, object> value, int charactersRead) ParseStructureValue(
            string text,
            int start,
            string whiteSpace = " \t\n\r",
            string separators = " \t\n\r,[]{}"
        )
            => ParseComposite<Dictionary<string, object>>(text, start, '{', '}', ParseStructureContent, whiteSpace, separators);
        

        public static (List<object> arrayValue, int charactersRead) ParseListValue(
            string text,
            int start,
            string whiteSpace = " \t\n\r",
            string separators = " \t\n\r,[]{}"
        )
        => ParseComposite<List<object>>(text, start, '[', ']', ParseListContent, whiteSpace, separators);


        private static (T value, int charactersRead) ParseComposite<T>(
            string text,
            int start,
            char compositeStart,
            char compositeEnd,
            Func<T, string, int,string, string, int> parseContentFunction,
            string whiteSpace = " \t\n\r",
            string separators = " \t\n\r,[]{}"
        ) where T : new()
        {           
            if (text[start] == compositeStart)
            {
                var result = new T();
                var idx = ParseCompositeElements(result, text, start + 1, compositeEnd, parseContentFunction, whiteSpace, separators);

                if (idx >= 0 && idx < text.Length && text[idx] == compositeEnd)
                {
                    return (result, (idx - start) + 1);
                }
                else
                {
                    Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find closing square bracket (']').");
                }

                return Error<T>();
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find opening square bracket ('[').");
                return Error<T>();  
            }
        }

        private static int ParseCompositeElements<T>(
            T result, 
            string text, 
            int idx, 
            char compositeEnd,
            Func<T, string, int, string, string, int> parseContentFunction,
            string whiteSpace, 
            string separators) where T : new()
        {
            while (idx >= 0 && idx < text.Length && text[idx] != compositeEnd)
            {
                idx = text.Skip(whiteSpace, idx);

                if (text[idx] != compositeEnd)
                {
                    idx = parseContentFunction(result, text, idx, whiteSpace, separators);                   
                }
            }

            return idx;
        }

        private static int ParseStructureContent(
            Dictionary<string, object> result, 
            string text, 
            int idx,
            string whiteSpace,
            string separators)
        {
            var (key, charactersRead) = ParseKey(text, idx, 0);

            if (key != default)
            {
                idx += charactersRead;
                var parseResult = ParseValue(text, idx, whiteSpace, separators);

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

                idx = text.Skip(whiteSpace, idx);

                if (idx < text.Length && text[idx] != '}')
                {
                    if (text[idx] != ',')
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
            string whiteSpace, 
            string separators
        ) 
        {
            var parseResult = ParseValue(text, idx, whiteSpace, separators);

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
            idx = text.Skip(whiteSpace, idx); 

            if (idx < text.Length && text[idx] != ']')
            {
                if (text[idx] != ',')
                {
                    Debug.LogWarning("PropertyTableParser.ParseArrayValue, missing separator.");
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

        public static (string stringValue, int charactersRead) ParseStringValue(string text, int start)
        {
            var firstCharacter = text[start];

            if (firstCharacter == '"' || firstCharacter == '\'')
            {
                var scopedStringLength = text.ReadScopedString(start, firstCharacter);

                if (scopedStringLength > 0)
                {
                    return (text.Substring(start + 1, scopedStringLength - 2), scopedStringLength);
                }
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseScopedStringValue: failed to parse string, missing beginning quotation mark.");
            }

            return Error<string>();
        }
    }
}
