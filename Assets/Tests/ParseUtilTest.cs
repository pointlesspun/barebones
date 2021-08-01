
using NUnit.Framework;
using System;

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

    [Test]
    [Description("Test parsing integers")]
    public void ParseIntegerNumberTest()
    {
        Assert.AreEqual(0, "0".ParseNumber());
        Assert.AreEqual(typeof(int), "0".ParseNumber().GetType());

        Assert.AreEqual(1, "1".ParseNumber());
        Assert.AreEqual(typeof(int), "1".ParseNumber().GetType());

        Assert.AreEqual(-42, "-42".ParseNumber());
        Assert.AreEqual(typeof(int), "-42".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing unsigned integers")]
    public void ParseUnsignedIntegerNumberTest()
    {
        Assert.AreEqual(0, "0u".ParseNumber());
        Assert.AreEqual(typeof(uint), "0u".ParseNumber().GetType());

        Assert.AreEqual(1, "1u".ParseNumber());
        Assert.AreEqual(typeof(uint), "1u".ParseNumber().GetType());

        Assert.AreEqual(2048, "2048u".ParseNumber());
        Assert.AreEqual(typeof(uint), "2048U".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing unsigned shorts")]
    public void ParseUnsignedShorts()
    {
        Assert.AreEqual(0, "0us".ParseNumber());
        Assert.AreEqual(typeof(ushort), "0us".ParseNumber().GetType());

        Assert.AreEqual(1, "1Us".ParseNumber());
        Assert.AreEqual(typeof(ushort), "1us".ParseNumber().GetType());

        Assert.AreEqual(2048, "2048uS".ParseNumber());
        Assert.AreEqual(typeof(ushort), "2048US".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing signed shorts")]
    public void ParseSignedShorts()
    {
        Assert.AreEqual(0, "0s".ParseNumber());
        Assert.AreEqual(typeof(short), "0s".ParseNumber().GetType());

        Assert.AreEqual(-1, "-1S".ParseNumber());
        Assert.AreEqual(typeof(short), "-1s".ParseNumber().GetType());

        Assert.AreEqual(2048, "2048S".ParseNumber());
        Assert.AreEqual(typeof(short), "2048S".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing unsigned longs")]
    public void ParseUnsignedLongs()
    {
        Assert.AreEqual(0, "0ul".ParseNumber());
        Assert.AreEqual(typeof(ulong), "0uL".ParseNumber().GetType());

        Assert.AreEqual(1, "1Ul".ParseNumber());
        Assert.AreEqual(typeof(ulong), "1UL".ParseNumber().GetType());

        Assert.AreEqual(20482048, "20482048ul".ParseNumber());
        Assert.AreEqual(typeof(ulong), "20482048Ul".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing signed longs")]
    public void ParseSignedLongs()
    {
        Assert.AreEqual(0, "0l".ParseNumber());
        Assert.AreEqual(typeof(long), "0l".ParseNumber().GetType());

        Assert.AreEqual(-1, "-1L".ParseNumber());
        Assert.AreEqual(typeof(long), "-1l".ParseNumber().GetType());

        Assert.AreEqual(-20482048, "-20482048L".ParseNumber());
        Assert.AreEqual(typeof(long), "-20482048L".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing hex numbers")]
    public void ParseHexNumberTest()
    {
        Assert.AreEqual(0, "0x0".ParseNumber());
        Assert.AreEqual(typeof(int), "0x0".ParseNumber().GetType());

        Assert.AreEqual(1, "0x1".ParseNumber());
        Assert.AreEqual(typeof(int), "0x1".ParseNumber().GetType());

        Assert.AreEqual(255, "0xFF".ParseNumber());
        Assert.AreEqual(typeof(int), "0xFF".ParseNumber().GetType());

        Assert.AreEqual(255, "0XFF".ParseNumber());
        Assert.AreEqual(typeof(int), "0XFF".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing byte numbers")]
    public void ParseByteTest()
    {
        Assert.AreEqual(0, "0b".ParseNumber());
        Assert.AreEqual(typeof(byte), "0b".ParseNumber().GetType());

        Assert.AreEqual(255, "255b".ParseNumber());
        Assert.AreEqual(typeof(byte), "255b".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing Decimal numbers")]
    public void ParseDecimalTest()
    {
        Assert.AreEqual(0, "0m".ParseNumber());
        Assert.AreEqual(typeof(decimal), "0m".ParseNumber().GetType());

        Assert.AreEqual(255.42, "255.42m".ParseNumber());
        Assert.AreEqual(typeof(decimal), "255.42m".ParseNumber().GetType());

        Assert.AreEqual(-0.42, "-0.42m".ParseNumber());
        Assert.AreEqual(typeof(decimal), "-0.42m".ParseNumber().GetType());

    }

    [Test]
    [Description("Test parsing floating point numbers")]
    public void ParseFloatTest()
    {
        Assert.AreEqual(0, "0f".ParseNumber());
        Assert.AreEqual(typeof(float), "0f".ParseNumber().GetType());

        Assert.AreEqual(255.42f, "255.42f".ParseNumber());
        Assert.AreEqual(typeof(float), "255.42f".ParseNumber().GetType());

        Assert.AreEqual(-0.42f, "-0.42f".ParseNumber());
        Assert.AreEqual(typeof(float), "-0.42f".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing double numbers")]
    public void ParseDoubleTest()
    {
        Assert.AreEqual(0, "0d".ParseNumber());
        Assert.AreEqual(typeof(double), "0d".ParseNumber().GetType());

        Assert.AreEqual(255.42, "255.42d".ParseNumber());
        Assert.AreEqual(typeof(double), "255.42d".ParseNumber().GetType());

        Assert.AreEqual(-0.42, "-0.42d".ParseNumber());
        Assert.AreEqual(typeof(double), "-0.42d".ParseNumber().GetType());

        Assert.AreEqual(0, "0.0".ParseNumber());
        Assert.AreEqual(typeof(double), "0.0".ParseNumber().GetType());

        Assert.AreEqual(255.42, "255.42".ParseNumber());
        Assert.AreEqual(typeof(double), "255.42".ParseNumber().GetType());

        Assert.AreEqual(-0.42, "-0.42".ParseNumber());
        Assert.AreEqual(typeof(double), "-0.42".ParseNumber().GetType());

        Assert.AreEqual(10.0e10, "10.0e10".ParseNumber());
        Assert.AreEqual(typeof(double), "10.0e10".ParseNumber().GetType());

        Assert.AreEqual(10e10, "10e10".ParseNumber());
        Assert.AreEqual(typeof(double), "10e10".ParseNumber().GetType());

        Assert.AreEqual(10e10d, "10e10d".ParseNumber());
        Assert.AreEqual(typeof(double), "10e10d".ParseNumber().GetType());
    }

    [Test]
    [Description("Test parsing with limitations")]
    public void ParseWithLimitationsTest()
    {
        // try hex, not allowed and only allowed
        Assert.Throws(typeof(ArgumentException), () => "0x0".ParseNumber("d"));
        Assert.AreEqual(1, "0x01".ParseNumber("x"));

        // try hex or double, not allowed and allowed
        Assert.Throws(typeof(ArgumentException), () => "12ul".ParseNumber("xd"));
        Assert.AreEqual(1, "0x01".ParseNumber("xd"));
        Assert.AreEqual(0.1, "0.1".ParseNumber("dx"));
        Assert.AreEqual(0.1, "0.1d".ParseNumber("dx"));

        // try usigned long
        Assert.AreEqual(12, "12ul".ParseNumber("xdul"));

        // try int, not allowed and allowed
        Assert.Throws(typeof(ArgumentException), () => "12".ParseNumber("b"));
        Assert.Throws(typeof(ArgumentException), () => "12z".ParseNumber("b"));
        Assert.AreEqual(1, "1".ParseNumber("bz"));
        Assert.AreEqual(1, "1z".ParseNumber("bz"));
    }
}