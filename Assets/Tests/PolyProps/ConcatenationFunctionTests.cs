using System.Collections.Generic;

using NUnit.Framework;

using BareBones.Services.PropertyTable;

public class ConcatenationFunctionTests
{
    [Test]
    [Description("provide an empty string, expect parsing to succeed with a failure as result")]
    public void TestEmptyString()
    {
        var basicFunctions = new BasicParseFunctions();
        var func = new ConcatenationParseFunction() {
            SkipWhiteSpaceFunction = basicFunctions.SkipWhiteSpaceAndComments
        }.Add(
            basicFunctions.NumberFunction,
            basicFunctions.StringFunction,
            new KeywordParseFunction()
            {
                Keyword = "bar",
            }
        );

        var result = func.Parse("");

        Assert.AreEqual(false, result.isSuccess);
        Assert.AreEqual(0, result.charactersRead);
        Assert.AreEqual(null, result.value);
    }

    [Test]
    [Description("provide an matching string, expect parsing to result in match")]
    public void TestMatchingString()
    {
        var basicFunctions = new BasicParseFunctions();
        var func = new ConcatenationParseFunction()
        {
            SkipWhiteSpaceFunction = basicFunctions.SkipWhiteSpaceAndComments
        }.Add(
            basicFunctions.NumberFunction,
            basicFunctions.StringFunction,
            new KeywordParseFunction()
            {
                Keyword = "bar",
            }
        );

        var text = "1  'foo'  bar";
        var result = func.Parse(text);

        Assert.AreEqual(true, result.isSuccess);
        Assert.AreEqual(text.Length, result.charactersRead);
        Assert.AreEqual(new List<object>() { 1, "foo", "bar" }, result.value);
    }

   
}
