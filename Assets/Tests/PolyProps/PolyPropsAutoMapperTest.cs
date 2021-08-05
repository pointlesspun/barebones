using System.Collections.Generic;

using NUnit.Framework;
using BareBones.Services.PropertyTable;
using System.Collections;

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

class SingleArrayFieldObject
{
    public int[] field = new int[] { 42 };
}

class SomethingOfEverything
{
    public int field1 = 42;
    public List<object> innerList = new List<object>() { new CompositeFieldObject() { innerObject = new PublicAndPrivatePrimitiveFieldObject() }, 42 };

    public Dictionary<string, object> Dict { get; set; } = new Dictionary<string, object>()
    {
        { "foo", "bar" },
        { "list", new List<string>() { "one", "two", "three" } },
        { "obj", new SingleArrayFieldObject() }
    };
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
    [Description("Test if a single array field is set correctly if the source is an array.")]
    public void SingleArrayFieldObjectTest()
    {
        var data = new Dictionary<string, object>()
        {
            { nameof(SingleArrayFieldObject.field), new int[] {1,2,3 } }
        };

        var obj = data.CreateInstance<SingleArrayFieldObject>();

        Assert.AreEqual(data[nameof(SingleArrayFieldObject.field)], obj.field);
    }

    [Test]
    [Description("Test if a single array field is set correctly if the source is a list.")]
    public void SingleArrayFromListFieldObjectTest()
    {
        var data = new Dictionary<string, object>()
        {
            { nameof(SingleArrayFieldObject.field), new List<int> {1,2,3 } }
        };

        var obj = data.CreateInstance<SingleArrayFieldObject>();

        Assert.AreEqual(data[nameof(SingleArrayFieldObject.field)], obj.field);
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

    [Test]
    [Description("Test if a IList with primitive values is deep copied correctly")]
    public void DeepCopyIListWithPrimitivesTest()
    {
        var source = (IList)new ArrayList() { 1, 2, "abc" };
        var list = source.DeepCopyList();

        Assert.IsTrue(list != source);
        Assert.AreEqual(list, source);
    }

    [Test]
    [Description("Test if a IList with nested list values is deep copied correctly")]
    public void DeepCopyIListWithNestedListTest()
    {
        var source = (IList)new ArrayList() { new ArrayList() { 1, 2, 3 }, 2, "abc" };
        var list = source.DeepCopyList();

        Assert.IsTrue(list != source);
        Assert.IsTrue(list[0] != source[0]);
        Assert.AreEqual(list, source);
    }

    [Test]
    [Description("Test if a nested IList with primitive values is deep copied correctly")]
    public void DeepCopyGenericListWithPrimitivesCheck()
    {
        var source = new List<int>() { 1, 2 };
        var list = source.DeepCopyList();

        Assert.IsTrue(list != source);
        Assert.AreEqual(list, source);
    }

    [Test]
    [Description("Test if a dictionary can be deep copied")]
    public void DeepCopyGenericDictionaryTest()
    {
        var source = new Dictionary<string, object>() { { "foo", "bar" },  { "int", 42 } };
        var copy = source.DeepCopy();

        Assert.IsTrue(copy != source);
        Assert.AreEqual(copy, source);
    }

    [Test]
    [Description("Test if a nested object with primitive values is deep copied correctly")]
    public void DeepCopyObjecttWithPrimitivesCheck()
    {
        var source = new PublicAndPrivatePrimitiveFieldObject();
        var result = source.DeepCopy();

        Assert.IsTrue(result != source);
        Assert.AreEqual(source.field1, result.field1);
        Assert.AreEqual(source.field2, result.field2);
        Assert.AreEqual(source.field3, result.field3);
    }

    [Test]
    [Description("Test is a random deep copy works as advertised")]
    public void DeepCopyWithSomeObjectWithEverything()
    {
        var source = new SomethingOfEverything();
        var result = source.DeepCopy();

        Assert.IsTrue(result != source);
        Assert.AreEqual(source.field1, result.field1);
        Assert.IsTrue(result.innerList != source.innerList);
    }
}
    