using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public static class PropertyTableParser
    {
        public static readonly char[] Eoln = new char[] { '\n', '\r' };

        public static readonly string Comma = "U+002C";
        public static readonly string QuotationMark = "U+0022";
        public static readonly string LeftSquareBracket = "U+005B";
        public static readonly string RightSquareBracket = "U+005D";

        public static readonly Dictionary<string, string> UnicodeTable = new Dictionary<string, string>()
        {
            {QuotationMark, "\""},
            {Comma, ","},
            {LeftSquareBracket, "["},
            {RightSquareBracket, "]"}
        };

        public static Dictionary<string, object> Read(TextAsset asset)
        {
            return Read(asset.text);
        }

        public static Dictionary<string, object> Read(string text)
        {
            return Read(text.Split(Eoln));
        }

        public static Dictionary<string, object> Read(string[] text)
        {
            var result = new Dictionary<string, object>();

            for (var i = 0; i < text.Length; i++)
            {
                var line = text[i];

                var columnIndex = line.IndexOf(':');

                if (columnIndex > 0)
                {
                    var key = line.Substring(0, columnIndex).Trim();

                    if (key.Length > 0 && key.IndexOf("//") != 0)
                    {
                        var value = line.Substring(columnIndex + 1).Trim();

                        try
                        {
                            result[key] = ParseValue(value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Exception while parsing value of key " + key + ", line + " + i);
                            Debug.LogWarning("Exception raised: " + e);
                        }
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, object> ReadProperties(string text)
        {
            var result = new Dictionary<string, object>();

            for (var i = 0; i < text.Length;)
            {
                var key = ParseKey(text, i, -1);

                if (key.key != String.Empty)
                {
                    /*i += key.length;

                    try
                    {
                        var value = ParseValue(text, i);
                        result[key.str] = value.value;
                        i += value.length + 1;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Exception while parsing value of key " + key + ", line + " + i);
                        Debug.LogWarning("Exception raised: " + e);
                    }*/
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static object ParseValue(string value)
        {
            if (value.Length == 0)
            {
                return null;
            }
            else if (value[0] == '"')
            {
                return ParseUnicodeString(value);
            }
            else if (value == "true" || value == "false")
            {
                return bool.Parse(value);
            }
            else if (char.IsDigit(value[0]) || value[0] == '-')
            {
                return ParseNumber(value);
            }
            else if (value[0] == '[')
            {
                return ParseArray(value);
            }

            return value;
        }

        private static object ParseArray(string value)
        {
            if (value[0] == '[' && value[value.Length - 1] == ']')
            {
                return ParseArrayValues(value.Substring(1, value.Length - 2));
            }

            throw new Exception("invalid array format");
        }

        private static object ParseArrayValues(string value)
        {
            var parts = value.Split(',');
            var result = new object[parts.Length];

            for (var i = 0; i < parts.Length; i++)
            {
                result[i] = ParseValue(parts[i].Trim());
            }

            return result;
        }

        private static object ParseNumber(string value)
        {
            if (value[value.Length - 1] == 'f')
            {
                return float.Parse(value.Substring(0, value.Length - 1));
            }
            else if (value.IndexOf('.') >= 0)
            {
                return float.Parse(value);
            }
            else
            {
                return int.Parse(value);
            }
        }

        private static string ParseUnicodeString(string value)
        {
            var length = value[value.Length - 1] == '"' ? value.Length - 1 : value.Length;
            var str = value.Substring(1, length - 1);
            var builder = new StringBuilder();
            var start = 0;
            var idx = str.IndexOf("U+");

            while (idx >= 0)
            {
                var token = str.Substring(idx, 6);

                builder.Append(str.Substring(start, idx - start));
                builder.Append(UnicodeTable[token]);

                start = idx + 6;
                idx = str.IndexOf("U+", start);
            }

            builder.Append(str.Substring(start));

            return builder.ToString();
        }

        public static (string key, int charactersRead) ParseKey(
            string text,
            int start,
            int line,
            string whiteSpace = " \t"
        )
        {
            var firstNonWhiteCharacter = text.IndexOfNone(whiteSpace, start);

            // case of an empty line
            if (firstNonWhiteCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteCharacter]) >= 0)
            {
                return ReadRemainderOfLine<string>(text, start);
            }

            // empty key case
            if (text[firstNonWhiteCharacter] == ':')
            {
                Debug.LogWarning("PropertyTableParser.ParseKey, empty key found at line " + line + ".");
                return ReadRemainderOfLine<string>(text, start);
            }

            var endOfKey = text.IndexOf(':', firstNonWhiteCharacter);

            // valid key found
            if (endOfKey >= 0)
            {
                return (text.Substring(start, endOfKey - start).Trim(), (endOfKey - start) + 1);
            }
            // case of no closing column
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseKey, key not delimited with a column ':' at line " + line + ".");
                return ReadRemainderOfLine<string>(text, start);
            }
        }

        private static (T value, int charactersRead) ReadRemainderOfLine<T>(
            string text,
            int start)
        {
            var nextEndOfLineOrEndOfFile = text.IndexOfAny("\n\r", start);
            var charactersRead = nextEndOfLineOrEndOfFile >= 0
                        ? (nextEndOfLineOrEndOfFile - start) + 1
                        : text.Length - start;
            return (default(T), charactersRead);
        }

        public static (object value, int charactersRead) ParsePropertyValue(
            string text,
            int start,
            string whiteSpace = " \t"
        )
        {
            var firstNonWhiteCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteCharacter]) >= 0)
            {
                return ReadRemainderOfLine<object>(text, start);
            }

            var whiteCharacterCount = firstNonWhiteCharacter - start;
            var character = text[firstNonWhiteCharacter];

            // try parse booleans
            if ((character == 't' || character == 'f') 
                && (ParseUtil.MatchLength(text, "true", firstNonWhiteCharacter) == 4
                || ParseUtil.MatchLength(text, "false", firstNonWhiteCharacter) == 5))
            {
                var (booleanValue, charactersRead) = ParsePODPropertyValue(text, firstNonWhiteCharacter, (str) => bool.Parse(str), whiteSpace);
                return (booleanValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a number
            else if (character == '-' || char.IsDigit(character))
            {
                var (numberValue, charactersRead) = ParsePODPropertyValue(text, firstNonWhiteCharacter, (str) => float.Parse(str), whiteSpace);
                return (numberValue, charactersRead + whiteCharacterCount);
            }
            // assume it's a string
            else
            {
                var (stringValue, charactersRead) = ParseStringPropertyValue(text, start, whiteSpace);
                return (stringValue, charactersRead + whiteCharacterCount);
            }
        }

        public static (object value, int charactersRead) ParseValue(
            string text,
            int start,
            string whiteSpace = " \t",
            string separators = " \t\n\r,[]{}"
        )
        {
            var firstNonWhiteCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteCharacter]) >= 0)
            {
                return ReadRemainderOfLine<object>(text, start);
            }

            var whiteCharacterCount = firstNonWhiteCharacter - start;
            var character = text[firstNonWhiteCharacter];

            // try parse booleans
            if ((character == 't' || character == 'f') // quick check before doing the heavier match
                && (ParseUtil.MatchLength(text, "true", firstNonWhiteCharacter) == 4
                || ParseUtil.MatchLength(text, "false", firstNonWhiteCharacter) == 5))
            {
                var (booleanValue, charactersRead) = 
                    ParsePODValue(text, firstNonWhiteCharacter, (str) => bool.Parse(str), whiteSpace, separators);
                return (booleanValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a number
            else if (character == '-' || char.IsDigit(character))
            {
                var (numberValue, charactersRead) = 
                    ParsePODValue(text, firstNonWhiteCharacter, (str) => float.Parse(str), whiteSpace, separators);
                return (numberValue, charactersRead + whiteCharacterCount);
            }
            // try parse an array
            else if (character == '[')
            {
                var (arrayValue, charactersRead) = ParseListValue(text, firstNonWhiteCharacter);
                return (arrayValue, charactersRead + whiteCharacterCount);
            }
            // try parse a structure
            else if (character == '{')
            {
                var (structValue, charactersRead) = ParseStructureValue(text, firstNonWhiteCharacter);
                return (structValue, charactersRead + whiteCharacterCount);
            }
            // try to parse a string
            else  if (character == '"' || character == '\'')
            {
                var (numberValue, charactersRead) = ParseScopedStringValue(text, firstNonWhiteCharacter);
                return (numberValue, charactersRead + whiteCharacterCount);
            }

            Debug.LogWarning("PropertyTableParser.ParseValue, unknown token encountered " + character + ".");
            return ReadRemainderOfLine<object>(text, start);
        }

        public static (Dictionary<string, object> value, int charactersRead) ParseStructureValue(
            string text,
            int start,
            string whiteSpace = " \t\n\r",
            string separators = " \t\n\r,[]{}"
        )
        {
            var firstNonWhiteSpaceCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteSpaceCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteSpaceCharacter]) >= 0)
            {
                var charactersRead = firstNonWhiteSpaceCharacter >= 0
                            ? (firstNonWhiteSpaceCharacter - start) + 1
                            : text.Length - start;
                return (null, charactersRead);
            }

            var result = new Dictionary<string, object>();

            if (text[firstNonWhiteSpaceCharacter] == '{')
            {
                var idx = firstNonWhiteSpaceCharacter + 1;
                while (idx < text.Length && text[idx] != '}')
                {
                    idx = text.IndexOfNone(whiteSpace, idx);

                    if (text[idx] != '}')
                    {
                        var keyParseResult = ParseKey(text, idx, 0);

                        if (keyParseResult.key != default(string)) 
                        {
                            idx += keyParseResult.charactersRead;
                            var parseResult = ParseValue(text, idx, whiteSpace, separators);

                            if (parseResult.value != null)
                            {
                                result[keyParseResult.key] = parseResult.value;
                                idx += parseResult.charactersRead;
                            }
                            else
                            {
                                Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed to parse value.");
                                return ReadRemainderOfLine<Dictionary<string, object>>(text, start);
                            }

                            var nextNonWhiteSpace = text.IndexOfNone(whiteSpace, idx);

                            if (nextNonWhiteSpace < 0)
                            {
                                Debug.LogWarning("PropertyTableParser.ParseStruct, malformed structure.");
                                return ReadRemainderOfLine<Dictionary<string, object>>(text, start);
                            }

                            idx = nextNonWhiteSpace;

                            if (idx < text.Length && text[idx] != '}')
                            {
                                if (text[idx] != ',')
                                {
                                    Debug.LogWarning("PropertyTableParser.ParseArrayValue, missing separator.");
                                    return ReadRemainderOfLine<Dictionary<string, object>>(text, start);
                                }

                                idx++;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("PropertyTableParser.ParseStructureValue, key is not valid.");
                            return ReadRemainderOfLine<Dictionary<string, object>>(text, start);
                        }

                    }
                }

                if (idx < text.Length && text[idx] == '}')
                {
                    return (result, (idx - start) + 1);
                }
                else
                {
                    Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find closing square bracket (']').");
                    return ReadRemainderOfLine<Dictionary<string, object>>(text, start);
                }
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find opening square bracket ('[').");
                return ReadRemainderOfLine<Dictionary<string, object>>(text, start);
            }
        }
    

        public static (List<object> arrayValue, int charactersRead) ParseListValue(
            string text,
            int start,
            string whiteSpace = " \t\n\r",
            string separators = " \t\n\r,[]{}"
        )
        {
            var firstNonWhiteSpaceCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteSpaceCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteSpaceCharacter]) >= 0)
            {
                var charactersRead = firstNonWhiteSpaceCharacter >= 0
                            ? (firstNonWhiteSpaceCharacter - start) + 1
                            : text.Length - start;
                return (null, charactersRead);
            }

            var result = new List<object>();

            if (text[firstNonWhiteSpaceCharacter] == '[')
            {
                var idx = firstNonWhiteSpaceCharacter + 1;
                while (idx < text.Length && text[idx] != ']')
                {
                    idx = text.IndexOfNone(whiteSpace, idx);

                    if (text[idx] != ']')
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
                            return ReadRemainderOfLine<List<object>>(text, start);
                        }

                        var nextNonWhiteSpace = text.IndexOfNone(whiteSpace, idx);

                        if (nextNonWhiteSpace < 0)
                        {
                            Debug.LogWarning("PropertyTableParser.ParseStruct, malformed structure.");
                            return ReadRemainderOfLine<List<object>>(text, start);
                        }

                        idx = nextNonWhiteSpace;

                        if (idx < text.Length && text[idx] != ']')
                        {
                            if (text[idx] != ',')
                            {
                                Debug.LogWarning("PropertyTableParser.ParseArrayValue, missing separator.");
                                return ReadRemainderOfLine<List<object>>(text, start);
                            }

                            idx++;
                        }
                    }
                } 

                if (idx < text.Length && text[idx] == ']')
                {
                    return (result, (idx - start) + 1);
                }
                else
                {
                    Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find closing square bracket (']').");
                    return ReadRemainderOfLine<List<object>>(text, start);
                }
            }
            else
            {
                Debug.LogWarning("PropertyTableParser.ParseArrayValue, failed find opening square bracket ('[').");
                return ReadRemainderOfLine<List<object>>(text, start);

            }
        }

        public static (object value, int charactersRead) ParsePODPropertyValue<T>(
            string text,
            int start,
            Func<string, T> parseFunction,
            string whiteSpace = " \t"
        )
        {
            var result = ParsePODValue(text, start, parseFunction, whiteSpace);

            // read remainder of line
            var eol = text.IndexOfAny("\n\r", start + result.charactersRead);
            var charactersRead = eol >= 0 ? (eol - start) + 1 : text.Length - start;

            return (result.value, charactersRead);
        }

        public static (object value, int charactersRead) ParsePODValue<T>(
            string text,
            int start,
            Func<string, T> parseFunction,
            string whiteSpace = " \t", 
            string endOfTokenCharacters = " \t\r\n"
        )
        {
            var firstNonWhiteCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteCharacter]) >= 0)
            {
                return ReadRemainderOfLine<object>(text, start);
            }

            var endOfToken = text.IndexOfAny(endOfTokenCharacters, firstNonWhiteCharacter);

            endOfToken = endOfToken >= 0 ? endOfToken : text.Length;

            try
            {
                var value = parseFunction(text.Substring(firstNonWhiteCharacter, endOfToken - firstNonWhiteCharacter));
                return (value, endOfToken - start);
            }
            catch (Exception e)
            {
                Debug.LogWarning("PropertyTableParser.ParsePODValue, failed to parse value. Exception: " + e);
                return ReadRemainderOfLine<object>(text, start);
            }
        }

        public static (string stringValue, int charactersRead) ParseScopedStringValue(
            string text,
            int start,
            string whiteSpace = " \t"
        )
        {
            var firstNonWhiteCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteCharacter]) >= 0)
            {
                var charactersRead = firstNonWhiteCharacter >= 0
                            ? (firstNonWhiteCharacter - start) + 1
                            : text.Length - start;
                return (null, charactersRead);
            }

            var firstCharacter = text[firstNonWhiteCharacter];

            if (firstCharacter == '"' || firstCharacter == '\'')
            {
                var scopedStringLength = text.ReadScopedString(firstNonWhiteCharacter, firstCharacter);

                if (scopedStringLength < 0)
                {
                    Debug.LogWarning("PropertyTableParser.ParseScopedStringValue, failed to parse string.");
                    return ReadRemainderOfLine<string>(text, start);
                }
                else
                {
                    return (text.Substring(firstNonWhiteCharacter + 1, scopedStringLength - 2),
                            (firstNonWhiteCharacter + scopedStringLength) - start);
                }
            }

            Debug.LogWarning("PropertyTableParser.ParseScopedStringValue, failed to parse string, missing quotes.");
            return ReadRemainderOfLine<string>(text, start);
        }

        /**
         * A string property value is the value defined from start until the end-of-line
         * or end-of-file. 
         * Cases handled:
         * 
         *  - 'key: \n' -> value = null
         *  - 'key: foo\n' -> value = "foo"
         *  - 'key: " foo "\n -> value = " foo " 
         *  - 'key: "foo\nbar"\n -> value = "foo\nbar"
         *  - 'key: "foo\"bar"\n -> value = "foo"bar"
         *  - 'key: "foo
         *           bar"\n -> value = "foo {line break followed by spaces} bar"
         *  - 'key: foo
         *           bar\n -> value = "foo" 
         */
        public static (string stringValue, int charactersRead) ParseStringPropertyValue(
            string text, 
            int start, 
            string whiteSpace = " \t"
        )
        {
            var firstNonWhiteCharacter = text.IndexOfNone(whiteSpace, start);

            // end-of-line or end-of-file?
            if (firstNonWhiteCharacter == -1 || "\n\r".IndexOf(text[firstNonWhiteCharacter]) >= 0)
            {
                var charactersRead = firstNonWhiteCharacter >= 0
                            ? (firstNonWhiteCharacter - start) + 1
                            : text.Length - start;
                return (null, charactersRead);
            }

            var firstCharacter = text[firstNonWhiteCharacter];

            if (firstCharacter == '"' || firstCharacter == '\'')
            {
                var scopedStringLength = text.ReadScopedString(firstNonWhiteCharacter, firstCharacter);
                
                if (scopedStringLength < 0)
                {
                    var nextEndOfLineOrEndOfFile = text.IndexOfAny("\n\r", firstNonWhiteCharacter);
                    var charactersRead = nextEndOfLineOrEndOfFile >= 0
                            ? (nextEndOfLineOrEndOfFile - start) + 1
                            : text.Length - start;
                    return (null, charactersRead);
                }
                else
                {
                    var nextEndOfLineOrEndOfFile = text.IndexOfAny("\n\r", firstNonWhiteCharacter + scopedStringLength);
                    var charactersRead = nextEndOfLineOrEndOfFile >= 0
                            ? (nextEndOfLineOrEndOfFile - start) + 1
                            : text.Length - start;
                    return (text.Substring(firstNonWhiteCharacter + 1, scopedStringLength - 2),
                            charactersRead);
                }
            }
            else
            {
                var nextEndOfLineOrEndOfFile = text.IndexOfAny("\n\r", firstNonWhiteCharacter);
                var charactersRead = nextEndOfLineOrEndOfFile >= 0
                            ? (nextEndOfLineOrEndOfFile - start) + 1
                            : text.Length - start;
                return (text.Substring(start, charactersRead).Trim(), charactersRead);
            }
        }
    }
}
