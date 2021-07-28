using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public class BasicParseFunctions
    {
        public static int SkipWhiteSpaceAndComments(string text, int start, string whiteSpace = " \t\r\n", string commentToken = "//")
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

        public Action<(int, int), string> Log { get; set; }

        public Func<string, int, int> SkipFunction { get; set; }

        public GroupParseFunction ValueFunction { get; set; }

        public AnyCharParseFunction AnyCharFunction { get; set; }

        public StringParseFunction StringFunction { get; set; }

        public GroupParseFunction KeyFunction { get; set; }

        public KeyValueParseFunction<string, object> KeyValueFunction { get; set; }

        public CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>> MapFunction { get; set; }

        public CompositeParseFunction<List<object>, object> ListFunction { get; set; }

        public NumberParseFunction NumberFunction { get; set; }

        public GroupParseFunction BoolFunction { get; set; }

        public KeywordParseFunction NullFunction {get;set;}
             
        public BasicParseFunctions(Action<(int, int), string> log = null)
        {
            Log = log;

            SkipFunction = new Func<string, int, int>((text, idx) => SkipWhiteSpaceAndComments(text, idx));

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

        public IPolyPropsParseFunction CreatePolyPropsFunction()
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
    }
}
