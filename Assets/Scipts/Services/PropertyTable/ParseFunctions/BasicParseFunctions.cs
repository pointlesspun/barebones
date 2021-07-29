using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class BasicParseFunctions
    {
        public string SingleLineCommentToken { get; set; } = "//";

        public Action<(int, int), string> Log { get; set; }

        public Func<string, int, int> SkipFunction { get; set; }

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

            SkipFunction = new Func<string, int, int>((text, idx) => SkipWhiteSpaceAndComments(text, idx, commentToken: SingleLineCommentToken));

            ValueFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipFunction
            };

            AnyCharFunction = new AnyCharParseFunction() { Log = log };

            StringFunction = new StringParseFunction() { Log = log };

            KeyFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipFunction
            }.Add(StringFunction, AnyCharFunction);

            KeyValueFunction = new KeyValueParseFunction<string, object>()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipFunction,
                KeyParseFunction = KeyFunction,
                ValueParseFunction = ValueFunction
            };

            MapFunction = new CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>>()
            {
                ElementParseFunction = KeyValueFunction,
                Log = log,
                SkipWhiteSpaceFunction = SkipFunction
            };

            ListFunction = new CompositeParseFunction<List<object>, object>()
            {
                ElementParseFunction = ValueFunction,
                Log = log,
                SkipWhiteSpaceFunction = SkipFunction,
                StartToken = "[",
                EndToken = "]"
            };

            NumberFunction = new NumberParseFunction() { Log = log };

            BoolFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = SkipFunction
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
                SkipWhiteSpaceFunction = SkipFunction
            };
            
            return new GroupParseFunction()
            {
                Log = Log,
                SkipWhiteSpaceFunction = SkipFunction
            }.Add(
                MapFunction,
                ListFunction,
                topLevelKeyValueFunction
            );
        }

        private int SkipWhiteSpaceAndComments(string text, int start, string whiteSpace = " \t\r\n", string commentToken = "//")
        {
            start = text.Skip(whiteSpace, start);

            while (start < text.Length && text.IsMatch(commentToken, start))
            {
                var eoln = text.IndexOfAny("\r\n", start);

                if (eoln >= 0)
                {
                    start = eoln + 1;
                    start = text.Skip(whiteSpace, start);
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
