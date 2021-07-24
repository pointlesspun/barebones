
using System;
using System.Collections.Generic;

using NUnit.Framework;
using BareBones.Services.PropertyTable;

public class PropertyTableParserTest
{
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
        var result = PolyPropsParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key :";
        result = PolyPropsParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " a b c :";
        result = PolyPropsParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == "a b c");
        Assert.IsTrue(result.charactersRead == testString.Length);

        var prefix = "keyA: bla\n";
        testString = " keyB :";
        result = PolyPropsParser.ParseKey(prefix + testString, prefix.Length, 0);

        Assert.IsTrue(result.key == "keyB");
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse key with a missing column.")]
    public void ParseMissingColumnTest()
    {
        var testString = "key";
        var result = PolyPropsParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == -1);

        testString = " key \n";
        result = PolyPropsParser.ParseKey(testString, 0, 0);

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
        var input = new string[] { "true", "false", "", " \n", "true", "'true' ", "fals", "TRUE" };
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
        var input = new string[] { " 0", " 0.1", "'foo'", "xxx", " true", " ' bar '", " -1.11" };
        var expectedValue = new object[] { 0.0f, 0.1f, "foo", null, true, " bar ", -1.11f};

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PolyPropsParser.ParseValue(testString, 0);

            if (expectedValue[i] == null)
            {
                Assert.IsTrue(value == expectedValue[i]);
                Assert.IsTrue(charactersRead == -1);
            }
            else
            {
                var actual = value;
                var expected = expectedValue[i];
                Assert.IsTrue(charactersRead == testString.Length);
                Assert.IsTrue(expected.Equals(actual));
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
            "['foo', 'bar']", 
            "[ not_valid ",
            "[ no comma ]",
        };

        var expectedValues = new List<object>[] {
            new List<object>() { true, false, 1, 2, -3, "bar\nbar" },
            new List<object>(),
            new List<object>() { 0.1f },
            new List<object>() { "foo", "bar" },
            null,
            null
        };

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PolyPropsParser.ParseListValue(testString, 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
                Assert.IsTrue(charactersRead == -1);
            }
            else
            {
                Assert.AreEqual(value, expectedValues[i]);
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
            "[ [no comma] ]",
        };

        var expectedValues = new List<object>[] {
            new List<object>() { new List<object>() { new List<object>() { -1} }, true, new List<object>() { 1,2,3} },
            new List<object>() { new List<object>() },
            new List<object>() { new List<object>() { 1 } },
            new List<object>() { new List<object>() { "foo" }, new List<object>() { "bar", "baz" } },
            null,
            null
        };

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PolyPropsParser.ParseListValue(testString, 0);

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

    [Test, Timeout(2000)]
    [Description("Parse an simple structure value.")]
    public void ParseSimpleStructureTest()
    {
        var input = new string[] {
            "{ key: 'no closing curlies'",
            "{\n key: 'value'\n}",
            "{\n key1: 'value1', \nkey2:\n'value2' \n}",
            "{\n key1: ['foo', 'bar'], \nkey2:\n[\n]\n, key 3: [ 1,2,3]}",
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
            new Dictionary<string, object>(),
            null,
            null
        };

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PolyPropsParser.ParseStructureValue(testString, 0);

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
            var (value, charactersRead) = PolyPropsParser.ParseStructureValue(testString, 0);

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
            var (value, charactersRead) = PolyPropsParser.ParsePODValue(testString, 0, (str) => parseFunction(str));

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
