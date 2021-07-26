
using System;
using System.Collections.Generic;

using NUnit.Framework;
using BareBones.Services.PropertyTable;
using UnityEngine;

class SinglePrimitiveFieldObject
{
    public int field = 42;
}

class PublicAndPrivatePrimitiveFieldObject
{
    public int field1 = 42;
    public string field2 = "foo";
    public bool field3 = false;
    private int field4 = 1;
    private string field5 = "bar";

    public int Field4 => field4;
    public string Field5 => field5;

}

class CompositeFieldObject
{
    public int field1 = 42;
    public PublicAndPrivatePrimitiveFieldObject innerObject;
}


public class PolyPropsAutoMapper
{
    [Test]
    [Description("Test if a single primitive field is set correctly.")]
    public void SingleFieldObjectTest() {
        var data = new Dictionary<string, object>()
        {
            { nameof(SinglePrimitiveFieldObject.field), -1 }
        };

        var obj = data.CreateInstance<SinglePrimitiveFieldObject>();

        Assert.AreEqual(obj.field, data[nameof(SinglePrimitiveFieldObject.field)]);
    }

    [Test]
    [Description("Test if a multiple primitive fields are set correctly.")]
    public void MultipleFieldObjectTest()
    {
        var data = new Dictionary<string, object>()
        {
            { nameof(PublicAndPrivatePrimitiveFieldObject.field1), -1 },
            { nameof(PublicAndPrivatePrimitiveFieldObject.field2), "bar" },
            { nameof(PublicAndPrivatePrimitiveFieldObject.field3), true },
            { "field4", -42 },
            { "field5", "foo" },
        };

        var obj = data.CreateInstance<PublicAndPrivatePrimitiveFieldObject>();

        Assert.AreEqual(data[nameof(PublicAndPrivatePrimitiveFieldObject.field1)], obj.field1);
        Assert.AreEqual(data[nameof(PublicAndPrivatePrimitiveFieldObject.field2)], obj.field2);
        Assert.AreEqual(data[nameof(PublicAndPrivatePrimitiveFieldObject.field3)], obj.field3);
        Assert.AreNotEqual(data["field4"], obj.Field4);
        Assert.AreNotEqual(data["field5"], obj.Field5);
    }

    [Test]
    [Description("Test if a multiple primitive fields are set correctly.")]
    public void InnerObjectInFieldObjectTest()
    {
        var innerData = new Dictionary<string, object>()
        {
            { nameof(PublicAndPrivatePrimitiveFieldObject.field1), -1 },
            { nameof(PublicAndPrivatePrimitiveFieldObject.field2), "bar" },
            { nameof(PublicAndPrivatePrimitiveFieldObject.field3), true },
        };

        var outerData = new Dictionary<string, object>()
        {
            { nameof(CompositeFieldObject.field1), -1 },
            { nameof(CompositeFieldObject.innerObject), innerData },
        };

        var outerObj = outerData.CreateInstance<CompositeFieldObject>();

        Assert.AreEqual(outerData[nameof(CompositeFieldObject.field1)], outerObj.field1);

        var innerObj = outerObj.innerObject;

        Assert.AreEqual(innerData[nameof(PublicAndPrivatePrimitiveFieldObject.field1)], innerObj.field1);
        Assert.AreEqual(innerData[nameof(PublicAndPrivatePrimitiveFieldObject.field2)], innerObj.field2);
        Assert.AreEqual(innerData[nameof(PublicAndPrivatePrimitiveFieldObject.field3)], innerObj.field3);
    }
}
    