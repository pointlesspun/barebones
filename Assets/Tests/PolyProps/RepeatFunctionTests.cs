using System.Collections.Generic;

using NUnit.Framework;

using BareBones.Services.PropertyTable;

public class RepeatFunctionTests
{
    [Test]
    [Description("provide an empty string, expect parsing to succeed with an empty list as result")]
    public void EmptyInputTest()
    {
        var result = RepeatParseFunction<List<object>, object>.Parse(null, null, null, null);

        Assert.AreEqual(result.isSuccess, true);
        Assert.AreEqual(result.value, new List<object>());
        Assert.AreEqual(result.charactersRead, 0);

        result = RepeatParseFunction<List<object>, object>.Parse("", null, null, null);

        Assert.AreEqual(result.isSuccess, true);
        Assert.AreEqual(result.value, new List<object>());
        Assert.AreEqual(result.charactersRead, 0);
    }

    [Test]
    [Description("provide an string with one element expect parsing to succeed with with one element as well")]
    public void SingleInputTest()
    {
        var functions = new BasicParseFunctions();
        var result = RepeatParseFunction<List<object>, object>.Parse("1", functions.NumberFunction, functions.SkipWhiteSpaceAndComments, null);

        Assert.AreEqual(result.isSuccess, true);
        Assert.AreEqual(result.value, new List<object>() { 1 });
        Assert.AreEqual(result.charactersRead, 1);

        result = RepeatParseFunction<List<object>, object>.Parse("1  ", functions.NumberFunction, functions.SkipWhiteSpaceAndComments, null);

        Assert.AreEqual(result.isSuccess, true);
        Assert.AreEqual(result.value, new List<object>() { 1 });
        Assert.AreEqual(result.charactersRead, 3);

    }

    [Test]
    [Description("provide an string with three elements expect parsing to succeed equal elements")]
    public void ThreeElementInputTest()
    {
        var functions = new BasicParseFunctions();
        var result = RepeatParseFunction<List<object>, object>.Parse("1, 2,  3", functions.NumberFunction, functions.SkipWhiteSpaceAndComments, null, (text, idx) => ",".IndexOf(text[idx]) >= 0 ? idx + 1 : -1);

        Assert.AreEqual(result.isSuccess, true);
        Assert.AreEqual(result.value, new List<object>() { 1, 2, 3 });
        Assert.AreEqual(result.charactersRead, 8);
    }

    [Test]
    [Description("provide an string with three elements and a terminator expect parsing to succeed equal elements")]
    public void ThreeElementsAndATerminatorInputTest()
    {
        var functions = new BasicParseFunctions();
        var result = RepeatParseFunction<List<object>, object>.Parse(
            "[1, 2,  3 ] ", 
            functions.NumberFunction, 
            functions.SkipWhiteSpaceAndComments,
            (text, idx) => text[idx] == ']' ? idx + 1 : -1, 
            (text, idx) => text[idx] == ',' ? idx + 1 : -1,
            start: 1
        );

        Assert.AreEqual(result.isSuccess, true);
        Assert.AreEqual(result.value, new List<object>() { 1, 2, 3 });
        Assert.AreEqual(10, result.charactersRead);
    }

    [Test]
    [Description("provide an string with three elements and miss a separator, expect the result to be a partial success (but still fail).")]
    public void ThreeElementAndAMistakeInputTest()
    {
        var testText = "[1, 2  3, 4 ]";
        var functions = new BasicParseFunctions();
        var result = RepeatParseFunction<List<object>, object>.Parse(
            testText,
            functions.NumberFunction,
            functions.SkipWhiteSpaceAndComments,
            (text, idx) => text[idx] == ']' ? idx + 1 : -1,
            (text, idx) => text[idx] == ',' ? idx + 1 : -1,
            start: 1
        );

        Assert.AreEqual(false, result.isSuccess);
        Assert.AreEqual(new List<object>() { 1, 2, 4 }, result.value);
        Assert.AreEqual(testText.Length - 1, result.charactersRead);
    }

    [Test]
    [Description("provide an string with two and three elements, set the min to three, one of these should succeed, one should fail.")]
    public void MinTest()
    {
        var testText1 = "1, 2, 3, 4]";
        var testText2 = "1, 2, 3]";
        var testText3 = "1, 2]";

        var functions = new BasicParseFunctions();
        var func = new RepeatParseFunction<List<object>, object>()
        {
            Function = functions.NumberFunction,
            SkipWhiteSpaceOperation = functions.SkipWhiteSpaceAndComments,
            TerminationOperation = (text, idx) => text[idx] == ']' ? idx + 1 : -1,
            SeparationOperation = (text, idx) => text[idx] == ',' ? idx + 1 : -1,
            Min = 3
        };

        var result = func.Parse(testText1);

        Assert.AreEqual(true, result.isSuccess);
        Assert.AreEqual(new List<object>() { 1, 2, 3, 4 }, result.value);
        Assert.AreEqual(testText1.Length, result.charactersRead);

        result = func.Parse(testText2);

        Assert.AreEqual(true, result.isSuccess);
        Assert.AreEqual(new List<object>() { 1, 2, 3 }, result.value);
        Assert.AreEqual(testText2.Length, result.charactersRead);

        result = func.Parse(testText3);

        Assert.AreEqual(false, result.isSuccess);
        Assert.AreEqual(null, result.value);
        Assert.AreEqual(testText3.Length, result.charactersRead);
    }

    [Test]
    [Description("provide an string with two and three elements, set the max to three, one of these should succeed, one should fail.")]
    public void MaxTest()
    {
        var testText1 = "1, 2, 3]";
        var testText2 = "1, 2]";
        var testText3 = "1]";

        var functions = new BasicParseFunctions();
        var func = new RepeatParseFunction<List<object>, object>()
        {
            Function = functions.NumberFunction,
            SkipWhiteSpaceOperation = functions.SkipWhiteSpaceAndComments,
            TerminationOperation = (text, idx) => text[idx] == ']' ? idx + 1 : -1,
            SeparationOperation = (text, idx) => text[idx] == ',' ? idx + 1 : -1,
            Max = 2
        };

        var result = func.Parse(testText2);

        Assert.AreEqual(true, result.isSuccess);
        Assert.AreEqual(new List<object>() { 1, 2 }, result.value);
        Assert.AreEqual(testText2.Length, result.charactersRead);

        result = func.Parse(testText3);

        Assert.AreEqual(true, result.isSuccess);
        Assert.AreEqual(new List<object>() { 1 }, result.value);
        Assert.AreEqual(testText3.Length, result.charactersRead);

        result = func.Parse(testText1);

        Assert.AreEqual(false, result.isSuccess);
        Assert.AreEqual(null, result.value);
        Assert.AreEqual(6, result.charactersRead);
    }
} 