using System.Collections.Generic;

using NUnit.Framework;
using BareBones.Services.PropertyTable;
using System.Collections;

class FooClass
{
    public string Text { get; set; } = "foo";
}

public class PolyPropsTest
{
    [Test]
    [Description("Test if a trivial empty object can be created.")]
    public void EmptyObjectTest()
    {
        var obj = PolyProps.CreateInstance<object>("object: {}");

        Assert.IsTrue(obj != null);
    }

    [Test]
    [Description("Test if a trivial Foo object can be created.")]
    public void FooTest()
    {
        var obj = (FooClass) PolyProps.CreateInstance("FooClass: { Text: 'bar' }");

        Assert.IsTrue(obj.Text == "bar");
    }
}
