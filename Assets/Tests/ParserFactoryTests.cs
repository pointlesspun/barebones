using System;

using NUnit.Framework;

using BareBones.Services.PropertyTable;
using UnityEngine;
using System.Collections.Generic;

public class ParseFactoryTest
{
    [Test]
    public void DefaultParserTrivialCasesTest()
    {
        var log = new Action<(int, int), string>((position, msg) => Debug.Log(position + ": " + msg));
        var parser = ParserFactory.PolyProps(log);

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
  
}
