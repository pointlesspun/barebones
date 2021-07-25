﻿
using System;
using System.Collections.Generic;

using NUnit.Framework;
using BareBones.Services.PropertyTable;
using UnityEngine;

public class PolyPropsParserTest
{
    [Test]
    [Description("Test if the extension collection handles all cases.")]
    public void ExtensionCollectionTest()
    {
        var text =
            "   // reading some colors and vectors\n" +
            "   c1: #0010FF,\n" +
            "   v1: v[1,2,3],\n" +
            "   foo: 'bar'\n" +
            "}";

        var config = PolyPropsExtensionCollection.CreateConfig(
            typeof(ParseVectorExtension),
            typeof(ParseColorExtension)
        );

        var value = PolyPropsParser.Read(text, config);
        var expected = new Dictionary<string, object>()
        {
            { "c1", new Color(0, 16.0f / 255.0f, 1.0f, 1.0f) },
            { "v1", new Vector3(1,2,3)},
            { "foo", "bar" },
        };
        Assert.AreEqual(expected, value);
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

        var colorParser = new ParseColorExtension();
        var config = new PolyPropsConfig()
        {
            CanParse = colorParser.CanParse,
            Parse = colorParser.Parse
        };
        var value = PolyPropsParser.Read(text, config);
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
    public void VectorExtensionTest()
    {
        var text = 
            "   // reading some vectors\n" +
            "   v2: v[1.0, 2],\n" +
            "   v3: v[1.0f, 2.0f, 3.0f],\n" +
            "   v4: v[1.0m, 2s, 3b, 4ul],\n" +
            "   foo: 'bar'\n" +
            "}";

        var vectorParser = new ParseVectorExtension();
        var config = new PolyPropsConfig()
        {
            CanParse = vectorParser.CanParse,
            Parse = vectorParser.Parse
        };
        var value = PolyPropsParser.Read(text, config);
        var expected = new Dictionary<string, object>()
        {
            { "v2", new Vector2(1.0f, 2.0f) },
            { "v3", new Vector3(1.0f, 2.0f, 3.0f) },
            { "v4", new Vector4(1.0f, 2.0f, 3.0f, 4.0f) },
            { "foo", "bar" },
        };
        Assert.AreEqual(expected, value);
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

        var value = PolyPropsParser.Read(new UnityEngine.TextAsset(text).text, config);
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

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key :";
        result = PolyPropsParser.ParseKey(testString, 0,  PolyPropsConfig.Default);

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key\n\n\n:";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "'key:':";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(result.key == "key:");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " a b c :";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(result.key == "a b c");
        Assert.IsTrue(result.charactersRead == testString.Length);

        var prefix = "keyA: bla\n";
        testString = " keyB :";
        result = PolyPropsParser.ParseKey(prefix + testString, prefix.Length, PolyPropsConfig.Default);

        Assert.IsTrue(result.key == "keyB");
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse key with a missing column.")]
    public void ParseMissingColumnTest()
    {
        var testString = "key";
        var result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == -1);

        testString = " key \n";
        result = PolyPropsParser.ParseKey(testString, 0, PolyPropsConfig.Default);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == -1);
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
    [Description("Parse boolean values.")]
    public void ParseBoolValueTest()
    {
        var input = new string[] { "true", "False", "", " \n", "true", "'true' ", "fals", "TRUE" };
        var expectedValue = new object[] { true, false, null, null, true, null, null, true };

        TestParseValues(input, expectedValue, (str) => bool.Parse(str));
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
            var (value, charactersRead) = PolyPropsParser.ParseValue(testString, 0, PolyPropsConfig.Default);

            if (expectedValue[i] == null)
            {
                Assert.IsTrue(value == expectedValue[i]);
                Assert.IsTrue(charactersRead == -1);
            }
            else
            {
                var actual = value;
                var expected = expectedValue[i];
                Assert.AreEqual(testString.Length, charactersRead);
                Assert.AreEqual(expected, actual);
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
            var (value, charactersRead) = PolyPropsParser.ParseList(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.AreEqual(expectedValues[i], value);
                Assert.AreEqual(-1, charactersRead);
            }
            else
            {
                Assert.AreEqual(expectedValues[i], value);
                Assert.IsTrue(charactersRead == testString.Length);
            }
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
            var (value, charactersRead) = PolyPropsParser.ParseList(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
            }
            else
            {
                Assert.AreEqual(value.Count, expectedValues[i].Count);
                Assert.AreEqual(value, expectedValues[i]);
                Assert.IsTrue(charactersRead == testString.Length);
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

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PolyPropsParser.ParseMap(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
                Assert.IsTrue(charactersRead == -1);
            }
            else
            {
                var result = (Dictionary<string, object>)value;
                Assert.AreEqual(result.Count, expectedValues[i].Count);
                Assert.AreEqual(expectedValues[i], result);
                Assert.IsTrue(charactersRead == testString.Length);
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
            var (value, charactersRead) = PolyPropsParser.ParseMap(testString, 0, PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
                Assert.IsTrue(charactersRead == -1);
            }
            else
            {
                var result = (Dictionary<string, object>)value;
                Assert.AreEqual(result.Count, expectedValues[i].Count);
                Assert.AreEqual(expectedValues[i], result);
                Assert.IsTrue(charactersRead == testString.Length);
            }
            
        }
    }

    private void TestParseValues<T>(string[] input, object[] expectedValues, Func<string, T> parseFunction)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PolyPropsParser.ParseValue(testString, 0, (str) => parseFunction(str), PolyPropsConfig.Default);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value.Equals(default(T)));
                Assert.IsTrue(charactersRead == -1);
            }
            else
            {
                var actual = value;
                var expected = expectedValues[i];
                Assert.IsTrue(expected.Equals(actual));
                Assert.IsTrue(charactersRead == testString.Length);
            }
        }
    }
}