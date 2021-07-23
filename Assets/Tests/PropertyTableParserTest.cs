
using NUnit.Framework;
using BareBones.Services.PropertyTable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropertyTableParserTest
{
    [Test]
    [Description("Parse a string with no or whitespace only characters.")]
    public void _ParseStringPropertyValueEmptyTest()
    {
        var testString = "";
        var result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == 0);

        testString = "   ";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "   \n";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "\t   \r";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);

        var key = "key:";

        testString = key + "   \n";
        result = PropertyTableParser.ParseStringPropertyValue(testString, key.Length);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length - key.Length);
    }

    [Test]
    [Description("Parse a end-of-line delimited string property.")]
    public void _ParseEndOfLineDelimitedStringTest()
    {
        var testString = "abc";
        var result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == testString);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " abc ";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == testString.Trim());
        Assert.IsTrue(result.charactersRead == testString.Length);

        var postFix = "def";
        testString = "abc\n";
        result = PropertyTableParser.ParseStringPropertyValue(testString + postFix, 0);

        Assert.IsTrue(result.stringValue == testString.Trim());
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "abc def \n";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == testString.Trim());
        Assert.IsTrue(result.charactersRead == testString.Length);

        var key = "key:";
        testString = " abc def";
        result = PropertyTableParser.ParseStringPropertyValue(key + testString, key.Length);

        Assert.IsTrue(result.stringValue == testString.Trim());
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse a valid quotation delimited string property.")]
    public void _ParseQuotationDelimitedStringTest()
    {
        var testString = "'abc'";
        var result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == "abc");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " ' abc ' ";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == " abc ");
        Assert.IsTrue(result.charactersRead == testString.Length);

        var postFix = "def";
        testString = "'abc'\n";
        result = PropertyTableParser.ParseStringPropertyValue(testString + postFix, 0);

        Assert.IsTrue(result.stringValue == "abc");
        Assert.IsTrue(result.charactersRead == testString.Length);

        var key = "key:";
        testString = "'abc' \n";
        result = PropertyTableParser.ParseStringPropertyValue(key + testString, key.Length);

        Assert.IsTrue(result.stringValue == "abc");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " 'abc\ndef' ";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == "abc\ndef");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " '' ";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == String.Empty);
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse an invalid quotation delimited string property.")]
    public void _ParseInvalidQuotationDelimitedStringTest()
    {
        var testString = "'abc";
        var result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " '";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " 'abc\n";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " 'abc\"";
        result = PropertyTableParser.ParseStringPropertyValue(testString, 0);

        Assert.IsTrue(result.stringValue == null);
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse an empty key.")]
    public void _ParseEmptyKeyTest()
    {
        var testString = "";
        var result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "   ";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "\n";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = "\nkey: bla";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == 1);

        testString = " :\n";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = ":";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse a valid key.")]
    public void _ParseValidKeyTest()
    {
        var testString = "key:";
        var result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key :";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == "key");
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " a b c :";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == "a b c");
        Assert.IsTrue(result.charactersRead == testString.Length);

        var prefix = "keyA: bla\n";
        testString = " keyB :";
        result = PropertyTableParser.ParseKey(prefix + testString, prefix.Length, 0);

        Assert.IsTrue(result.key == "keyB");
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    [Test]
    [Description("Parse key with a missing column.")]
    public void _ParseMissingColumnTest()
    {
        var testString = "key";
        var result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);

        testString = " key \n";
        result = PropertyTableParser.ParseKey(testString, 0, 0);

        Assert.IsTrue(result.key == default(string));
        Assert.IsTrue(result.charactersRead == testString.Length);
    }

    private void TestParseValues<T>(string[] input, object[] expectedValues, Func<string, T> parseFunction)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PropertyTableParser.ParsePODPropertyValue(testString, 0, (str) => parseFunction(str));

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
            }
            else
            {
                var actual = value;
                var expected = expectedValues[i];
                Assert.IsTrue(expected.Equals(actual));
            }
            Assert.IsTrue(charactersRead == testString.Length);
        }
    }

    [Test]
    [Description("Parse int values.")]
    public void _ParseIntValueTest()
    {
        var input = new string[] { "42", " -1", "", " \n", "444\n", "a38", " 0001", " 3282f"};
        var expectedValue = new object[] { 42, -1, null, null, 444, null, 1, null };

        TestParseValues(input, expectedValue, (str) => int.Parse(str));
    }

    [Test]
    [Description("Parse boolean values.")]
    public void _ParseBoolValueTest()
    {
        var input = new string[] { "true", " false ", "", " \n", " true\n", "'true' ", " fals", " TRUE" };
        var expectedValue = new object[] { true, false, null, null, true, null, null, true };

        TestParseValues(input, expectedValue, (str) => bool.Parse(str));
    }

    [Test]
    [Description("Parse float values.")]
    public void _ParseFloatValueTest()
    {
        var input = new string[] { "0", " 0.1 ", "", " \n", " 42.4\n", "'3' ", " -1.11", " 1e10" };
        var expectedValue = new object[] { 0.0f, 0.1f, null, null, 42.4f, null, -1.11f, 1e10f};

        TestParseValues(input, expectedValue, (str) => float.Parse(str));
    }

    [Test]
    [Description("Parse any value.")]
    public void _ParseAnyValueTest()
    {
        var input = new string[] { " 0", " 0.1", "'foo'", "", " true", " ' bar '", " -1.11" };
        var expectedValue = new object[] { 0.0f, 0.1f, "foo", null, true, " bar ", -1.11f};

        for (var i = 0; i < input.Length; i++)
        {
            var testString = input[i];
            var (value, charactersRead) = PropertyTableParser.ParseValue(testString, 0);

            if (expectedValue[i] == null)
            {
                Assert.IsTrue(value == expectedValue[i]);
            }
            else
            {
                var actual = value;
                var expected = expectedValue[i];
                Assert.IsTrue(expected.Equals(actual));
            }
            Assert.IsTrue(charactersRead == testString.Length);
        }
    }

    [Test]
    [Description("Parse a list value.")]
    public void _ParseListTest()
    {
        var input = new string[] {
            " [ true, \nfalse, 1, 2, -3, 'bar\nbar']",
            "[]", 
            "[ 0.1 ]", 
            " ['foo', 'bar']", 
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
            var (value, charactersRead) = PropertyTableParser.ParseListValue(testString, 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
            }
            else
            {
                Assert.AreEqual(value.Count, expectedValues[i].Count);
                for (var j = 0; j < value.Count; j++)
                {
                    var actual = value[j];
                    var expected = expectedValues[i][j];
                    Assert.AreEqual(expected, actual);
                }
            }
            Assert.IsTrue(charactersRead == testString.Length);
        }
    }

    [Test]
    [Description("Parse an nested list value.")]
    public void _ParseNestedListTest()
    {
        var input = new string[] {

            "[ [ [-1] ], true,  [ 1,2,3 ]  ]",
            " [ [] ]",
            "[ [ 1 ]]",
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
            var (value, charactersRead) = PropertyTableParser.ParseListValue(testString, 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
            }
            else
            {
                Assert.AreEqual(value.Count, expectedValues[i].Count);
                for (var j = 0; j < value.Count; j++)
                {
                    var actual = value[j];
                    var expected = expectedValues[i][j];

                    Assert.AreEqual(expected, actual);
                }
            }
            Assert.IsTrue(charactersRead == testString.Length);
        }
    }

    [Test, Timeout(2000)]
    [Description("Parse an simple structure value.")]
    public void _ParseSimpleStructureTest()
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
            var (value, charactersRead) = PropertyTableParser.ParseStructureValue(testString, 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
            }
            else
            {
                Assert.AreEqual(value.Count, expectedValues[i].Count);
                Assert.AreEqual(expectedValues[i], value);
            }
            Assert.IsTrue(charactersRead == testString.Length);
        }
    }

    [Test, Timeout(2000)]
    [Description("Parse an nested structure value.")]
    public void _ParseNestedStructureTest()
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
            var (value, charactersRead) = PropertyTableParser.ParseStructureValue(testString, 0);

            if (expectedValues[i] == null)
            {
                Assert.IsTrue(value == expectedValues[i]);
            }
            else
            {
                Assert.AreEqual(value.Count, expectedValues[i].Count);
                Assert.AreEqual(expectedValues[i], value);
            }
            Assert.IsTrue(charactersRead == testString.Length);
        }
    }
}
