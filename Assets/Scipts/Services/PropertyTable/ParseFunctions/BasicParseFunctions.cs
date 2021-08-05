using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class BasicParseFunctions
    {
        public string SingleLineCommentToken { get; set; } = "//";

        public string WhiteSpaceCharacters { get; set; } = " \t\r\n";

        public Action<(int, int), string> Log { get; set; }

        /**
         * Parse a 'value' in the text at the given start position. A value can either be a standard value
         * (boolean, number, list (array), map (object), string, null or a string without delimiters OR
         * one of the extension types specified in the configuration.
         */
        public GroupParseFunction ValueFunction { get; set; }

        public AnyCharParseFunction AnyCharFunction { get; set; }

        public StringParseFunction StringFunction { get; set; }

        /** 
         * Parse a 'key' of a key-value pair at the given position in the text. The key either starts 
         * with a string delimiter (usually ' or ") and the key is read as a string or otherwise
         * the key is read as an undelimited string until a PolyPropsConfig.UnquotedStringsDelimiters 
         * is encountered.
         */
        public GroupParseFunction KeyFunction { get; set; }

        public KeyValueParseFunction<string, object> KeyValueFunction { get; set; }

        /**
         * Parses a map (or object/struct), a collection of key/value pairs, delimited by PolyPropsConfig.MapDelimiters 
         * and separated by PolyPropsConfig.CompositeValueSeparator.
         */
        public CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>> MapFunction { get; set; }

        /**
         * Parses a list of values delimited by PolyPropsConfig.ListDelimiters 
         * and separated by PolyPropsConfig.CompositeValueSeparator.
         */
        public CompositeParseFunction<List<object>, object> ListFunction { get; set; }

        public NumberParseFunction NumberFunction { get; set; }

        public GroupParseFunction BoolFunction { get; set; }

        public KeywordParseFunction NullFunction {get;set;}
             
        public BasicParseFunctions(Action<(int, int), string> log = null)
        {
            Log = log;

            ValueFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments
            };

            AnyCharFunction = new AnyCharParseFunction() { Log = log };

            StringFunction = new StringParseFunction() { Log = log };

            KeyFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments
            }.Add(StringFunction, AnyCharFunction);

            KeyValueFunction = new KeyValueParseFunction<string, object>()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
                KeyParseFunction = KeyFunction,
                ValueParseFunction = ValueFunction
            };

            MapFunction = new CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>>()
            {
                ElementParseFunction = KeyValueFunction,
                Log = log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments
            };

            ListFunction = new CompositeParseFunction<List<object>, object>()
            {
                ElementParseFunction = ValueFunction,
                Log = log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
                StartToken = "[",
                EndToken = "]"
            };

            NumberFunction = new NumberParseFunction() { Log = log };

            BoolFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments
            }.Add(
                new KeywordParseFunction() { Log = log, Keyword = "true", ValueFunction = () => true },
                new KeywordParseFunction() { Log = log, Keyword = "false", ValueFunction = () => false }
            );

            NullFunction = new KeywordParseFunction() { Log = log, Keyword = "null", ValueFunction = () => null };

            ValueFunction.Add(
                BoolFunction,
                NullFunction,
                NumberFunction,
                MapFunction,
                ListFunction,
                StringFunction
            );

            ValueFunction.DefaultFunction = AnyCharFunction;
        }

        public IParseFunction CreatePolyPropsFunction()
        {
            // define a meta group, this only allows maps or lists on the top level
            var topLevelKeyValueFunction = new CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>>()
            {
                ElementParseFunction = KeyValueFunction,
                StartToken = String.Empty,
                EndToken = String.Empty,
                Log = Log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments
            };
            
            return new GroupParseFunction()
            {
                Log = Log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments
            }.Add(
                MapFunction,
                ListFunction,
                topLevelKeyValueFunction
            );
        }

        public IParseFunction CreateXMLFunction()
        {
            var stack = new List<Regex>();

            var attributeFunction = new KeyValueParseFunction<string, object>()
            {
                Name = "XmlAttribute",
                KeyParseFunction = new AnyCharParseFunction()
                {
                    Delimiters = "= ",
                    EscapeChar = (char)0,
                    Log = Log
                },
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
                Log = Log,
                SeparatorToken = "=",
                ValueParseFunction = StringFunction
            };

            var xmlNodeFunction = new ConcatenationParseFunction()
            {
                Name = "XmlNode",
                Log = Log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
                AddNullElements = false
            };

            var xmlTagFunction = new AnyCharParseFunction()
            {
                Name = "XmlTag",
                Delimiters = " />",
                Log = Log,
                EscapeChar = (char)0,
                OnMatch = (str) => {

                    stack.Insert(0, new Regex("\\G<\\/\\s*" + str + "\\s*>"));
                }
            };

            var xmlAttributeCollection = new RepeatParseFunction<Dictionary<string, object>, KeyValuePair<string, object>>()
            {
                Name = "XmlAttributeCollection",
                Log = Log,
                SkipWhiteSpaceOperation = SkipWhiteSpaceAndComments,
                Function = attributeFunction,
                TerminationOperation = MatchAnyChar(" />"),
                ConsumeTermination = false
            };

            var xmlNodeOpen = new KeywordParseFunction()
            {
                Name = "XmlNodeOpen",
                Keyword = "<",
                Log = Log,
                ValueFunction = () => null,
            };

            var xmlChildElement = new GroupParseFunction()
            {
                Name = "XmlChildElement",
                Log = Log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
            }.Add(
                xmlNodeFunction,
                new AnyCharParseFunction()
                {
                    Delimiters = "<",
                    Log = Log,
                    EscapeChar = (char)0,
                }
            );

            var xmlChildren = new RepeatParseFunction<List<object>, object>()
            {
                Name = "XmlChildren",
                Function = xmlChildElement,
                Log = Log,
                AddNullElements = false,
                SkipWhiteSpaceOperation = SkipWhiteSpaceAndComments,
                TerminationOperation = (text, start) =>
                {
                    var match = stack[0].Match(text, start);

                    if (match.Success)
                    {
                        stack.RemoveAt(0);
                        return start + match.Length;
                    }

                    return -1;
                }
            };

            var xmlComposite = new ConcatenationParseFunction()
            {
                Name = "XmlComposite",
                Log = Log,
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
                AddNullElements = false,
                CompressResult = true
            }.Add(
                new KeywordParseFunction()
                {
                    Keyword = ">",
                    Log = Log,
                    ValueFunction = () => null,
                },
                xmlChildren);

            var xmlNodeClose = new GroupParseFunction()
            {
                Name = "XmlNodeClose",
                SkipWhiteSpaceFunction = SkipWhiteSpaceAndComments,
                Log = Log,
            }.Add(
                new KeywordParseFunction()
                {
                    Keyword = "/>",
                    Log = Log,
                    ValueFunction = () => null,
                    Name = "XmlClose",
                    OnMatch = (str) => stack.RemoveAt(0)
                },
                xmlComposite                
            );

            xmlNodeFunction.Add(
                xmlNodeOpen,
                xmlTagFunction,
                xmlAttributeCollection,
                xmlNodeClose
            );

            return xmlNodeFunction;
        }

        public ParseOperation MatchAnyChar(string characters)
            => (text, start) => characters.IndexOf(text[start]) >= 0 ? start + 1 : -1;


        public ParseOperation MatchChar(char c) 
            => (text, start) => text[start] == c ? start + 1 : -1;

        public int SkipWhiteSpaceAndComments(string text, int start)
        {

            start = text.Skip(WhiteSpaceCharacters, start);

            while (start < text.Length && text.IsMatch(SingleLineCommentToken, start))
            {
                var eoln = text.IndexOfAny("\r\n", start);

                if (eoln >= 0)
                {
                    start = eoln + 1;
                    start = text.Skip(WhiteSpaceCharacters, start);
                }
                else
                {
                    start = text.Length;
                }
            }

            return start;
        }
    }
}
