using System;
using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public static class ParserFactory
    {
        public static int SkipWhiteSpaceAndComments(string text, int start, string whiteSpace = " \t\r\n", string commentToken = "//" )
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

        public static IPolyPropsParseFunction CreateBasicParser(Action<(int, int), string> log = null)
        {
            var skipFunction = new Func<string, int, int>((text, idx) => SkipWhiteSpaceAndComments(text, idx));

            var valueParserFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = skipFunction
            };

            var anyFunction = new AnyCharParseFunction() { Log = log };
            var stringFunction = new StringParseFunction() { Log = log };

            valueParserFunction.DefaultFunction = anyFunction;

            // key can be either with quotes or without
            var keyFunction = new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = skipFunction
            }.Add(stringFunction, anyFunction);

            var keyValuePairParseFunction = new KeyValueParseFunction<string, object>()
            {
                Log = log,
                SkipWhiteSpaceFunction = skipFunction,
                KeyParseFunction = keyFunction,
                ValueParseFunction = valueParserFunction
            };

            var mapFunction = new CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>>()
            {
                ElementParseFunction = keyValuePairParseFunction,
                Log = log,
                SkipWhiteSpaceFunction = skipFunction
            };

            var listFunction = new CompositeParseFunction<List<object>, object>()
            {
                ElementParseFunction = valueParserFunction,
                Log = log,
                SkipWhiteSpaceFunction = skipFunction,
                StartToken = "[",
                EndToken = "]"
            };

            valueParserFunction.Add(
                new KeywordParseFunction() {  Log = log, Keyword = "true", ValueFunction = () => true},
                new KeywordParseFunction() { Log = log, Keyword = "false", ValueFunction = () => false },
                new KeywordParseFunction() { Log = log, Keyword = "null", ValueFunction = () => null},
                new NumberParseFunction() { Log = log },
                mapFunction,
                listFunction,
                stringFunction
            );

            var topLevelKeyValueFunction = new CompositeParseFunction<Dictionary<string, object>, KeyValuePair<string, object>>()
            {
                ElementParseFunction = keyValuePairParseFunction,
                StartToken = String.Empty,
                EndToken = String.Empty,
                Log = log,
                SkipWhiteSpaceFunction = skipFunction
            };

            // define a meta group, this only allows maps or lists on the top level
            return new GroupParseFunction()
            {
                Log = log,
                SkipWhiteSpaceFunction = skipFunction
            }.Add(
                mapFunction,
                listFunction,
                topLevelKeyValueFunction
            );
        }
    }
}
