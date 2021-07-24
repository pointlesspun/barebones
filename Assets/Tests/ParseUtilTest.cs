
using NUnit.Framework;

public class ParseUtilTest
{
    [Test]
    [Description("Test whether or not the right column and line is returned")]
    public void LineAndColumnTest()
    {
        Assert.AreEqual((0, 0), "".GetLineAndColumn(0));
        Assert.AreEqual((0, 0), "1".GetLineAndColumn(0));
        Assert.AreEqual((0, 1), "1".GetLineAndColumn(1));
        Assert.AreEqual((1, 0), "1\n".GetLineAndColumn(1));
        Assert.AreEqual((0, 1), "12\n".GetLineAndColumn(1));
        Assert.AreEqual((1, 0), "12\n".GetLineAndColumn(2));
        Assert.AreEqual((1, 0), "12\n1".GetLineAndColumn(3));
        Assert.AreEqual((1, 1), "12\n12".GetLineAndColumn(4));
        Assert.AreEqual((2, 0), "12\n12\n".GetLineAndColumn(5));
    }
}   