using System;

using NUnit.Framework;

using BareBones.Services.PropertyTable;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ParseFactoryTest
{
    [Test]
    public void DefaultParserTrivialCasesTest()
    {
        var log = new Action<(int, int), string>((position, msg) => Debug.Log(position + ": " + msg));
        var parser = new BasicParseFunctions(log).CreatePolyPropsFunction();

        var result = parser.Parse("", 0);

        Assert.IsTrue(result.isSuccess);
        Assert.AreEqual(result.value, null);

        // cannot parse standalone values, must be map or list or keyvalue pair
        result = parser.Parse("true", 0);
        Assert.IsTrue(!result.isSuccess);

        // should be able to parse standalone key value pairs
        result = parser.Parse("'key': true,\n'value': 123", 0);
        Assert.IsTrue(result.isSuccess);
        Assert.AreEqual(new Dictionary<string, object>()
        {
            { "key", true },
            { "value", 123 }
        }, result.value);

        result = parser.Parse("{'key':value, 'foo': bar, 'answer': 42.01, 'null': null, 'true' : true, 'false': false }", 0);

        Assert.IsTrue(result.isSuccess);
        Assert.AreEqual(new Dictionary<string, object>()
        {
            { "key", "value" },
            { "foo", "bar" },
            { "answer", 42.01 },
            { "null", null },
            { "true", true },
            { "false", false },
        }, result.value);

        // key should be able to be parsed without quotes
        result = parser.Parse("{key:value}", 0);

        Assert.IsTrue(result.isSuccess);
        Assert.AreEqual(new Dictionary<string, object>()
        {
            { "key", "value" }
        }, result.value);

        result = parser.Parse("// just a comment", 0);
        Assert.IsTrue(result.isSuccess);
        Assert.AreEqual(null, result.value);

        // test comments and a list
        result = parser.Parse("// just a list\n[1, 'two', true]", 0);

        Assert.IsTrue(result.isSuccess);
        Assert.AreEqual(new List<object>()
        {
            1, "two", true
        }, result.value);
    }

    [Test]
    public void ParserExtensionTest()
    {
        var log = new Action<(int, int), string>((position, msg) => Debug.Log(position + ": " + msg));
        var basicFunctions = new BasicParseFunctions(log);

        basicFunctions.ValueFunction.Add(
            new VectorParseFunction()
            {
                ListFunction = basicFunctions.ListFunction,
                Log = basicFunctions.Log,
            },
            new ColorParseFunction()
            {
                Log = basicFunctions.Log
            }
        );

        var text =
            @"  // example of extensions
                key: value,
                vector: v[1,2,3],
                color: #AABBCC,
                map: {
                    // inner map values
                    key: 'value of text',
                    list: [1,2,3]
                },
                boolean: true, // always has been ?

                // closing with nothing
                void: null
            ";

        var result = basicFunctions.CreatePolyPropsFunction().Parse(text);
        var value = result.value as Dictionary<string, object>;

        foreach (var kvp in new Dictionary<string, object>()
        {
            {"key", "value"},
            { "vector", new Vector3(1,2,3) },
            { "color", new Color( 0xAA / 255.0f, 0xBB / 255.0f, 0xCC / 255.0f ) },
            { "map", new Dictionary<string, object>()
                    {
                        { "key", "value of text" },
                        { "list", new List<object>() { 1, 2, 3 } },
                    }
            },
            { "boolean", true },
            { "void", null }
        })
        {
            Assert.AreEqual(kvp.Value, value[kvp.Key]);
        }
    }

    [Test]
    public void XmlSimpleNodeTests()
    {
        var log = new Action<(int, int), string>((position, msg) => Debug.Log(position + ": " + msg));
        var basicFunctions = new BasicParseFunctions(log);
        var xmlParser = basicFunctions.CreateXMLFunction();

        var simpleNode = "<node/>";
        var parseResult = xmlParser.Parse(simpleNode);

        Assert.AreEqual(true, parseResult.isSuccess);
        Assert.AreEqual(simpleNode.Length, parseResult.charactersRead);
        Assert.AreEqual(new List<object>() { "node", new Dictionary<string, object>() }, parseResult.value);

        var nodeWithAttributes = "<node attr1='value1' attr2 = 'value2' />";
        parseResult = xmlParser.Parse(nodeWithAttributes);

        Assert.AreEqual(true, parseResult.isSuccess);
        Assert.AreEqual(nodeWithAttributes.Length, parseResult.charactersRead);
        Assert.AreEqual(new List<object>() { "node", new Dictionary<string, object>() {
            {"attr1", "value1"},
            {"attr2", "value2"},
        } }, parseResult.value);
    }

    [Test]
    public void XmlCompositeNodeTests()
    {
        var log = new Action<(int, int), string>((position, msg) => Debug.Log(position + ": " + msg));
        var basicFunctions = new BasicParseFunctions(log);
        var xmlParser = basicFunctions.CreateXMLFunction();

        var simpleNode = "<node><child/></node>";
        var parseResult = xmlParser.Parse(simpleNode);
        var expectedChildNode = new List<object>() {
                    "child",
                    new Dictionary<string, object>()
                };
        var expected = new List<object>() {
            "node",
            new Dictionary<string, object>(),
            new List<object>()
            {
                expectedChildNode
            }
        };
        Assert.AreEqual(true, parseResult.isSuccess);
        Assert.AreEqual(simpleNode.Length, parseResult.charactersRead);
        Assert.AreEqual(expected, parseResult.value);
    }
}