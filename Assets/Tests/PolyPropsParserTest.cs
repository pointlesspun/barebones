
using System;
using System.Collections.Generic;

using NUnit.Framework;
using BareBones.Services.PropertyTable;
using UnityEngine;
using System.Linq;

public class PolyPropsParserTest
{
    [Test]
    [Description("Test if read function reports the first error correctly.")]
    public void ReportErrorTest()
    {
        var text =
            "{\n" +
            "  // no column between key and value\n" +
            "  'c1' #0010FF,\n" +
            "}";

        var errors = new List<((int line, int column) position, string message)>();
        var config = new PolyPropsConfig();

        config.Log = (position, message) => errors.Add((position, message));

        var value = PolyPropsParser.Read(text, 0, config);
        Assert.Greater(errors.Count, 0);
        Assert.AreEqual(errors[0].position.line, 2);
        Assert.AreEqual(errors[0].position.column, 7);
    }

     [Test]
    [Description("Test if the color extension is capable of parsing all the colors.")]
    public void ColorExtensionTest()
    {
        var text =
            "   // reading some colors\n" +
            "   c1: #0010FF,\n" +
            "   c2: #0010FF01,\n" +
            "   foo: 'bar'\n" +
            "}";

        var config = new PolyPropsConfig()
        {
            ParseExtensions = new ColorParseFunction()
        };
        
        var value = PolyPropsParser.Read(text, 0, config);
        var expected = new Dictionary<string, object>()
        {
            { "c1", new Color(0, 16.0f / 255.0f, 1.0f, 1.0f) },
            { "c2", new Color(0, 16.0f / 255.0f, 1.0f, 1.0f / 255.0f) },
            { "foo", "bar" },
        };
        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if the vector extension is capable of parsing all the vector types.")]
    public void VectorFunctionTest()
    {
        var text = new string[] {
            "v<1.0, 2>",
            "v<1.0f, 2.0f, 3.0f>",
            "v<1.0m, 2s, 3b, 4ul>",          
        };

        var log = new Action<(int, int), string>((pos, msg) => Debug.Log(pos + ": " + msg));

        var function = new VectorParseFunction()
        {
            ListFunction = new CompositeParseFunction<List<object>, object>()
            {
                StartToken = "<",
                EndToken = ">",
                ElementParseFunction = new NumberParseFunction()
                {
                    Delimiters = NumberParseFunction.DefaultDelimiters + ">",
                    Log = log
                },
                SkipWhiteSpaceFunction = (text, idx) => ParseUtil.Skip(text, " \n", idx),
                Log = log
            },
            VectorPrefix = "v<",
            Log = log
        };

        var expected = new object[] {
            new Vector2(1.0f, 2.0f),
            new Vector3(1.0f, 2.0f, 3.0f),
            new Vector4(1.0f, 2.0f, 3.0f, 4.0f)
        };

        for (var  i = 0; i < text.Length; i++)
        {
            Assert.AreEqual(expected[i], function.Parse(text[i], 0).value);
        }

    }

    [Test]
    [Description("Test if a top level map is parsed correctly.")]
    public void ReadMapTest()
    {
        var text =
            "// Reading a map\n" +
            "{\n" +
            "   // comment\n" +
            "   str: 'string',\n" +
            "   key-value: { key: value },\n" +
            "   float: 128.0f,\n" +
            "   null: null\n" +
            "}";

        var value = PolyPropsParser.Read(text);
        var expected = new Dictionary<string, object>()
        {
            {"str", "string"},
            {"key-value", new Dictionary<string, object>()
                {
                    {"key", "value" },
                }
            },
            {"float", 128.0f },
            {"null", null}
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if a top level list is parsed correctly.")]
    public void ReadListTest()
    {
        var text =
            "// Reading a list\n" +
            "[\n" +
            "   // comment\n" +
            "   'string',\n" +
            "   { key: value },\n" + 
            "   128.0f\n" +  
            "]";

        var value = PolyPropsParser.Read(text);
        var expected = new List<object>()
        {
            "string",
            new Dictionary<string, object>()
            {
                {"key", "value" },
            },
            128.0f
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in between key value pairs is ignored.")]
    public void SingleLineCommentInBetweenInnerKeyValuePairsTest()
    {
        var text =
            "key: {\n" +
            "   // comment   \n" +
            "   k1:// comment\n" +
            "'value1' // comment\n" +
            "// comment\n" +
            ",  k2: 'value2', // comment\n" +
            "   k3: \"value3\" // comment\n" +
            "} // comment";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key", new Dictionary<string, object>() 
                {
                    {"k1", "value1" },
                    {"k2", "value2" },
                    {"k3", "value3" },
                }
            },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in between list values is ignored.")]
    public void SingleLineCommentInBetweenListValuesTest()
    {        
        var text =
            "key1: [\n" +
            "'value1' // comment\n" +
            ", 'value2', // comment\n" +
            "'value3' // comment\n" +
            "] // comment";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key1", new List<object>() { "value1", "value2", "value3" } },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in after list start is ignored.")]
    public void SingleLineCommentAfterListStartTest()
    {
        var text =
            "key1: [// comment \n" +
            "'value']";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key1", new List<object>() { "value" } },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in after the key is ignored.")]
    public void SingleLineCommentInBetweenKeyAndValueTest()
    {
        var text =
            "key1: // comment \n" +
            "'value'";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key1", "value" },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in after the key and before the separator is leading to weird keys.")]
    public void SingleLineCommentInBetweenKeyAndColumnTest()
    {
        var text =
            "key1 // comment \n" +
            ": 'value'";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key1", "value" },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in after the key/value pairs are indeed ignored.")]
    public void SingleLineCommentOnSameLineAfterKeyValuePairsTest()
    {
        var text =
            "key1: 'value', # comment \n" +
            "key2: TRUE #comment \n," +
            "\n" +
            "key3: 42\n ## more comments \n,";

        // change the comment token for some variation
        var config = new PolyPropsConfig()
        {
            SingleLineCommentToken = "#"
        };

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text, 0, config);
        var expected = new Dictionary<string, object>()
        {
            { "key1", "value" },
            { "key2", true },
            { "key3", 42 },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment in between key/value pairs are indeed ignored.")]
    public void SingleLineCommentInBetweenKeyValuePairsTest()
    {
        var text =
            "key1: 'value',\n" +
            "// this are some single\n" +
            "key2: tRuE\n," +
            "\n" +
            "// comments\n" +
            "key3: 42\n," +
            "// ...\n" +
            "\n" +
            "key4: -42\n";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key1", "value" },
            { "key2", true },
            { "key3", 42 },
            { "key4", -42 },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment at the end of a text are indeed ignored.")]
    public void SingleLineCommentAtEndTest()
    {
        var text =
            "   key: 'value'\n" +
            " // this are some single\n" +
            "// line\n" +
            "  // comments\n";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key", "value" },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Test if single line comment at the beginning of a text are indeed ignored.")]
    public void SingleLineCommentAtBeginningTest()
    {
        var text = 
            " // this are some single\n" +
            "// line\n" +
            "  // comments\n" +
            "   key: 'value'";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "key", "value" },
        };

        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Parse a valid text asset.")]
    public void ReadTextAsset()
    {
        var text =
            "foo: 'bar'," +
            "list: [1, 2, 3]," +
            "object: {" +
            "   objKey: 'value'," +
            "   objFlag: true," +
            "}";

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text);
        var expected = new Dictionary<string, object>()
        {
            { "foo", "bar" },
            { "list", new List<object>() {1,2,3} },
            {
                "object",
                new Dictionary<string, object>()
                {
                    {"objKey", "value"},
                    {"objFlag", true }
                }
            }
        };
        
        Assert.AreEqual(expected, value);
    }

    [Test]
    [Description("Parse a valid key.")]
    public void ParseValidKeyTest()
    {
        var testString = "key:";
        var result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.AreEqual(result.value, "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key :";
        result = PolyPropsParser.ParseKey(testString, 0,  PolyPropsConfig.Default);

        Assert.AreEqual(result.value, "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key\n\n\n:";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.AreEqual(result.value, "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "'key:':";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.AreEqual(result.value, "key:");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " a b c :";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.AreEqual(result.value, "a b c");
        Assert.IsTrue(result.charactersRead == testString.Length);

        var prefix = "keyA: bla\n";
        testString = " keyB :";
        result = PolyPropsParser.ParseKey(prefix + testString, prefix.Length, PolyPropsConfig.Default);

        Assert.AreEqual(result.value, "keyB");
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse key with a missing column.")]
    public void ParseMissingColumnTest()
    {
        var testString = "key";
        var result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(!result.isSuccess);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key \n";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(!result.isSuccess);
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse int values.")]
    public void ParseIntValueTest()
    {
        var input = new string[] { "42", "-1", "", " \n", "444", "a38", "0001", " 282f"};
        var expectedValue = new object[] { 42, -1, null, null, 444, null, 1, null };

        TestParseValues(input, expectedValue, (str) => int.Parse(str));
    }

    [Test]
    [Description("Use the parse function to parse int values.")]
    public void ParseIntFunctionTest()
    {
        var input = new string[] { "42", "-1", "", " \n", "444", "a38", "0001", " 282f" };
        var expectedValues = new object[] { 42, -1, null, null, 444, null, 1, null };

        for (var i = 0; i < input.Length; i++)
        {
            var result = NumberParseFunction.Parse(input[i], 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                Assert.AreEqual(expectedValues[i], result.value);
            }
        }
    }


    [Test]
    [Description("Use the key-value parse function to parse string-int values.")]
    public void ParseKeyValueFunctionTest()
    {
        var input = new string[] { "'a': 1", "'foo' :\n 42", "'bar':", " 'fail' 3", "'fail'", };
        var expectedValues = new object[] { 
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("foo", 42),
            new KeyValuePair<string, int>("bar", default(int)),
            null,
            null
        }; 

        var keyValueParseFunction = new KeyValueParseFunction<string, int>()
        {
            KeyParseFunction = new StringParseFunction(),
            ValueParseFunction = new NumberParseFunction(),
            SkipWhiteSpaceFunction = (text, idx) => ParseUtil.Skip(text, " \n", idx),
        };

        for (var i = 0; i < input.Length; i++)
        {
            var result = keyValueParseFunction.Parse(input[i], 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                Assert.AreEqual(expectedValues[i], result.value);
            }
        }
    }

    [Test]
    [Description("Parse boolean values.")]
    public void ParseBoolValueTest()
    {
        var input = new string[] { "true", "False", "", " \n", "true", "'true' ", "fals", "TRUE" };
        var expectedValue = new object[] { true, false, null, null, true, null, null, true };

        TestParseValues(input, expectedValue, (str) => bool.Parse(str));
    }

    [Test]
    [Description("Use the parse function to parse bool values.")]
    public void ParseBoolFunctionTest()
    {
        var input = new string[] { "true", "False", "", " \n", "true", "'true' ", "fals", "TRUE" };
        var expectedValues = new object[] { true, false, null, null, true, null, null, true };

        for (var i = 0; i < input.Length; i++)
        {
            var result = BooleanParseFunction.Parse(input[i], 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                Assert.AreEqual(expectedValues[i], result.value);
            }
        }
    }

    [Test]
    [Description("Parse float values.")]
    public void ParseFloatValueTest()
    {
        var input = new string[] { "0", "0.1", "", " \n", "42.4", "'3'", "-1.11", "1e10" };
        var expectedValue = new object[] { 0.0f, 0.1f, null, null, 42.4f, null, -1.11f, 1e10f};

        TestParseValues(input, expectedValue, (str) => float.Parse(str));
    }

    [Test]
    [Description("Parse any value.")]
    public void ParseAnyValueTest()
    {
        var input = new string[] { "0", "0.1", "'foo'", "xxx", "true", "\" bar \"", "-1.11f" };
        var expectedValue = new object[] { 0, 0.1, "foo", "xxx", true, " bar ", -1.11f};

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var result = PolyPropsParser.ParseValue(testString, 0, PolyPropsConfig.Default);

            var actual = result.value;
            var expected = expectedValue[i];
            Assert.AreEqual(testString.Length, result.charactersRead);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    [Description("Parse any value via a group parse.")]
    public void GroupParseFunctionTest()
    {
        var input = new string[] { "0", "0.1", "'foo'", "xxx", "true", "\" bar \"", "-1.11f" };
        var expectedValues = new object[] { 0, 0.1, "foo", null, true, " bar ", -1.11f };

        var groupFunctions = new GroupParseFunction().Add(
            new NumberParseFunction(),
            new BooleanParseFunction(),
            new StringParseFunction()
        );

        for (var i = 0; i < input.Length; i++)
        {
            var result = groupFunctions.Parse(input[i]);

            if (expectedValues[i] == null)
            {                
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                Assert.IsTrue(groupFunctions.CanParse(input[i]));
                Assert.AreEqual(expectedValues[i], result.value);
            }
        }
    }

    [Test]
    [Description("Parse a list value.")]
    public void ParseListTest()
    {
        var input = new string[] {
            "[ true, \nfalse, 1, 2, -3, 'bar\nbar']",
            "[]", 
            "[ 0.1 ]",
            "[ 0.1, 0xff, -42.0f]",
            "['foo', \"bar\"]", 
            "[ unlimited \n]",
            "[ not_valid ",
            "[ 'no' 'comma' ]",
        };

        var expectedValues = new List<object>[] {
            new List<object>() { true, false, 1, 2, -3, "bar\nbar" },
            new List<object>(),
            new List<object>() { 0.1 },
            new List<object>() { 0.1, 255, -42f },
            new List<object>() { "foo", "bar" },
            new List<object>() { "unlimited" },
            null,
            null
        };

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var result = PolyPropsParser.ParseList(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.AreEqual(expectedValues[i], result.value);
                //Assert.AreEqual(-1, result.charactersRead);
            }
            else
            {
                Assert.AreEqual(expectedValues[i], result.value);
                Assert.IsTrue(result.charactersRead == testString.Length);
            }
        }
    }

    [Test]
    [Description("Parse a list value via the composite parse function.")]
    public void CompositeNumberListFunctionTest()
    {
        var input = new string[] {
            "[]",
            "[ 0.1 ]",
            "[ 0.1,0xff, -42.0f  ]",
        };

        var expectedValues = new List<object>[] {
            new List<object>(),
            new List<object>() { 0.1 },
            new List<object>() { 0.1, 255, -42f },
        };
      
        var parseFunction = new CompositeParseFunction<List<object>, object>()
        {
            ElementParseFunction = new NumberParseFunction(),
            StartToken = "[",
            EndToken = "]",
            SkipWhiteSpaceFunction = new Func<string, int, int>((str, idx) => ParseUtil.Skip(str, " \n\t\r", idx))
        };
        
        for (var i = 0; i < input.Length; i++)
        {
            var result = parseFunction.Parse(input[i]);
            Assert.AreEqual(expectedValues[i], result.value);
        }
    }


    [Test]
    [Description("Parse a list value via the composite parse function, omitting a start token.")]
    public void CompositeNumberListNoStartTokenFunctionTest()
    {
        var inputNoStartingDelimiter = new string[] {
            ";",
            "0.1;",
            "0.1 0xff -42.0f;",
        };

        var expectedValues = new List<object>[] {
            new List<object>(),
            new List<object>() { 0.1 },
            new List<object>() { 0.1, 255, -42f },
        };

        var parseFunction = new CompositeParseFunction<List<object>, object>()
        {
            ElementParseFunction = new NumberParseFunction() { Delimiters = NumberParseFunction.DefaultDelimiters + ";" },
            StartToken = String.Empty,
            EndToken = ";",
            ElementSeparators = " ",
            SkipWhiteSpaceFunction = new Func<string, int, int>((str, idx) => ParseUtil.Skip(str, "\n\r", idx))
        };

        for (var i = 0; i < inputNoStartingDelimiter.Length; i++)
        {
            var result = parseFunction.Parse(inputNoStartingDelimiter[i]);
            Assert.AreEqual(expectedValues[i], result.value);
        }
    }

    [Test]
    [Description("Parse a list value via the composite parse function, omitting an end token.")]
    public void CompositeNumberListNoEndTokenFunctionTest()
    {
        var inputNoStartingDelimiter = new string[] {
            "#",
            "#0.1",
            "#0.1 |0xff| -42.0f",
        };

        var expectedValues = new List<object>[] {
            new List<object>(),
            new List<object>() { 0.1 },
            new List<object>() { 0.1, 255, -42f },
        };

        var parseFunction = new CompositeParseFunction<List<object>, object>()
        {
            ElementParseFunction = new NumberParseFunction() { Delimiters = NumberParseFunction.DefaultDelimiters + "|" },
            StartToken = "#",
            EndToken = String.Empty,
            ElementSeparators = "|",
            SkipWhiteSpaceFunction = new Func<string, int, int>((str, idx) => ParseUtil.Skip(str, " \t\n\r", idx))
        };

        for (var i = 0; i < inputNoStartingDelimiter.Length; i++)
        {
            var result = parseFunction.Parse(inputNoStartingDelimiter[i]);
            Assert.AreEqual(expectedValues[i], result.value);
        }
    }

    [Test]
    [Description("Parse a list value via the composite parse function, omitting both start and end tokens.")]
    public void CompositeNumberListNoStartAndEndTokenFunctionTest()
    {
        var inputNoStartingDelimiter = new string[] {
            "",
            "0.1",
            "0.1:\n 0xff :\n -42.0f",
        };

        var expectedValues = new List<object>[] {
            new List<object>(),
            new List<object>() { 0.1 },
            new List<object>() { 0.1, 255, -42f },
        };

        var parseFunction = new CompositeParseFunction<List<object>, object>()
        {
            ElementParseFunction = new NumberParseFunction() { Delimiters = NumberParseFunction.DefaultDelimiters + ":" },
            StartToken = String.Empty,
            EndToken = String.Empty,
            ElementSeparators = ":",
            SkipWhiteSpaceFunction = new Func<string, int, int>((str, idx) => ParseUtil.Skip(str, " \t\n\r", idx))
        };

        for (var i = 0; i < inputNoStartingDelimiter.Length; i++)
        {
            var result = parseFunction.Parse(inputNoStartingDelimiter[i]);
            Assert.AreEqual(expectedValues[i], result.value);
        }
    }

    [Test]
    [Description("Parse an nested list value.")]
    public void ParseNestedListTest()
    {
        var input = new string[] {

            "[   [ [-1] ], true,  [ 1,2, \n3]  ]",
            "[ [] ]",
            "[ [ 1]]",
            "[ [ 'foo' ], ['bar','baz'] ]",          
            "[ [not_valid ] ",
            "[ ['no' 'comma'] ]",
            "[ [str, 'also, str', str too] ]",
        };

        var expectedValues = new List<object>[] {
            new List<object>() { new List<object>() { new List<object>() { -1} }, true, new List<object>() { 1,2,3} },
            new List<object>() { new List<object>() },
            new List<object>() { new List<object>() { 1 } },
            new List<object>() { new List<object>() { "foo" }, new List<object>() { "bar", "baz" } },
            null,
            null,
            new List<object>() { new List<object>() { "str", "also, str", "str too" } },
        };

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var result = PolyPropsParser.ParseList(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(result.isSuccess == false);
            }
            else
            {
                Assert.AreEqual(((List<object>)result.value).Count, expectedValues[i].Count);
                Assert.AreEqual(result.value, expectedValues[i]);
                Assert.IsTrue(result.charactersRead == testString.Length);
            }
            
        }
    }

    [Test]
    [Description("Parse an simple structure value.")]
    public void ParseSimpleStructureTest()
    {
        var input = new string[] {
            "{ key: 'no closing curlies'",
            "{\n key: 'value'\n}",
            "{\n key1: 'value1', \nkey2:\n'value2' \n}",
            "{\n key1: ['foo', 'bar'], \nkey2:\n[\n]\n, key 3: [ 1,2,3]}",
            "{key: value}",
            "{\n key: value // comment kicking in\n}",
            "{}",
            "{",
            "{ key: 'no' key2: 'comma' }",
        };

        var expectedValues = new Dictionary<string, object>[] {
            null,
            new Dictionary<string, object>()
            {
                {"key", "value"}
            },
            new Dictionary<string, object>()
            {
                {"key1", "value1"},
                {"key2", "value2"}
            },
            new Dictionary<string, object>()
            {
                {"key1", new List<object>() { "foo", "bar" } },
                {"key2", new List<object>()},
                {"key 3", new List<object>() {1,2,3} }
            },
            new Dictionary<string, object>()
            {
                {"key", "value"}
            },
            new Dictionary<string, object>()
            {
                {"key", "value"}
            },
            new Dictionary<string, object>(),
            null,
            null
        };

        PolyPropsConfig.Default.Log = (location, message) => Debug.Log(location + ": " + message);

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var result = PolyPropsParser.ParseMap(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                var dictionary = (Dictionary<string, object>)result.value;
                Assert.AreEqual(dictionary.Count, expectedValues[i].Count);
                Assert.AreEqual(expectedValues[i], dictionary);
                Assert.IsTrue(result.charactersRead == testString.Length);
            }
            
        }
    }

    [Test, Timeout(2000)]
    [Description("Parse an nested structure value.")]
    public void ParseNestedStructureTest()
    {
        var input = new string[] {
            "{key:{key:'value'}}",
            "{key3:{key2-1:{key:'value'}\n}}",
            "{key1:{key:'value'}, key2  \n:\n[1,2,3] ,key3:{key2-1:{key:'value'}\n}}",
        };

        var expectedValues = new Dictionary<string, object>[] {
            new Dictionary<string, object>()
            {
                {
                    "key", new Dictionary<string, object>()
                    {
                        {"key", "value"}
                    }
                }
            },
            new Dictionary<string, object>()
            {
                {
                    "key3",
                    new Dictionary<string, object>()
                    {
                        {
                            "key2-1",
                            new Dictionary<string, object>()
                            {
                                {"key", "value"}
                            }
                        }
                    }
                }
            },
            new Dictionary<string, object>()
            {
                {
                    "key1", 
                    new Dictionary<string, object>()
                    {
                        {"key", "value"}
                    }
                },
                {
                    "key2",
                    new List<object>()
                    {
                        1,2,3
                    }
                },
                {
                    "key3",
                    new Dictionary<string, object>()
                    {
                        {
                            "key2-1",
                            new Dictionary<string, object>()
                            {
                                {"key", "value"}
                            }
                        }
                    }
                },
            }
        };

        for (var i = 0; i < input.Length; i++)
        {       
            var testString = input[i];
            var result = PolyPropsParser.ParseMap(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                var dictionary = (Dictionary<string, object>)result.value ;
                Assert.AreEqual(dictionary.Count, expectedValues[i].Count);
                Assert.AreEqual(expectedValues[i], dictionary);
                Assert.IsTrue(result.charactersRead == testString.Length);
            }
        }
    }

    private void TestParseValues<T>(string[] input, object[] expectedValues, Func<string, T> parseFunction)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var result = PolyPropsParser.ParseValue(testString, 0, (str) => parseFunction(str), PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(!result.isSuccess);
            }
            else
            {
                var actual = result.value;
                var expected = expectedValues[i];
                Assert.IsTrue(expected.Equals(actual));
                Assert.IsTrue(result.charactersRead == testString.Length);
            }
        }
    }
}
