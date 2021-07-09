using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestSlotArray
{
    [Test]
    public void ConstructorTest()
    {
        var array = new SlotArray<object>(2);

        Assert.AreEqual(2, array.Capacity);
        Assert.AreEqual(2, array.Available);
        Assert.AreEqual(-1, array.First);

        Assert.IsNull(array[0]);
        Assert.IsNull(array[1]);
    }

    [Test]
    public void ObtainSingleTest()
    {
        var array = new SlotArray<object>(2);
        var obj1 = new object();

        Assert.AreEqual(0, array.Obtain(obj1));
        Assert.AreEqual(1, array.Available);
        Assert.AreEqual(0, array.First);
        Assert.AreEqual(-1, array.Next(0));
        Assert.AreEqual(-1, array.Next(1));

        Assert.AreEqual(obj1, array[0]);
        Assert.IsNull(array[1]);
    }

    [Test]
    public void ObtainMultipleTest()
    {
        var array = new SlotArray<object>(3);
        var objects = new object[] { new object(), new object(), new object() };

        Assert.AreEqual(0, array.Obtain(objects[0]));
        Assert.AreEqual(1, array.Obtain(objects[1]));
        Assert.AreEqual(2, array.Obtain(objects[2]));
        Assert.AreEqual(-1, array.Obtain(objects[2]));

        Assert.AreEqual(0, array.Available);
        Assert.AreEqual(2, array.First);
        Assert.AreEqual(1, array.Next(2));
        Assert.AreEqual(0, array.Next(1));
        Assert.AreEqual(-1, array.Next(0));

        Assert.AreEqual(objects[0], array[0]);
        Assert.AreEqual(objects[1], array[1]);
        Assert.AreEqual(objects[2], array[2]);
    }

    [Test]
    public void ObtainAndReleaseSingleTest()
    {
        var array = new SlotArray<object>(2);
        var obj1 = new object();

        Assert.AreEqual(0, array.Obtain(obj1));

        array.Release(0);

        Assert.AreEqual(2, array.Available);
        Assert.AreEqual(-1, array.First);
        
        Assert.IsNull(array[0]);
        Assert.IsNull(array[1]);
    }

    [Test]
    public void ObtainAndReleaseFirstTest()
    {
        var array = new SlotArray<object>(3);
        var obj1 = new object();
        var obj2 = new object();

        Assert.AreEqual(0, array.Obtain(obj1));
        Assert.AreEqual(1, array.Obtain(obj2));

        array.Release(0);

        Assert.AreEqual(2, array.Available);
        Assert.AreEqual(1, array.First);

        Assert.IsNull(array[0]);
        Assert.AreEqual(obj2, array[1]);
        Assert.IsNull(array[2]);
    }

    [Test]
    public void ObtainAndReleaseLastTest()
    {
        var array = new SlotArray<object>(3);
        var obj1 = new object();
        var obj2 = new object();

        Assert.AreEqual(0, array.Obtain(obj1));
        Assert.AreEqual(1, array.Obtain(obj2));

        array.Release(1);

        Assert.AreEqual(2, array.Available);
        Assert.AreEqual(0, array.First);

        Assert.AreEqual(obj1, array[0]);
        Assert.IsNull(array[1]);
        Assert.IsNull(array[2]);
    }

    [Test]
    public void ObtainAndReleaseMiddleTest()
    {
        var array = new SlotArray<object>(3);
        var obj1 = new object();
        var obj2 = new object();
        var obj3 = new object();

        Assert.AreEqual(0, array.Obtain(obj1));
        Assert.AreEqual(1, array.Obtain(obj2));
        Assert.AreEqual(2, array.Obtain(obj3));

        array.Release(1);

        Assert.AreEqual(1, array.Available);
        Assert.AreEqual(2, array.First);

        Assert.AreEqual(obj1, array[0]);
        Assert.IsNull(array[1]);
        Assert.AreEqual(obj3, array[2]);

        Assert.AreEqual(0, array.Next(2));
        Assert.AreEqual(-1, array.Next(0));
    }

    [Test]
    public void ReleaseAndObtainMiddleTest()
    {
        var array = new SlotArray<object>(3);
        var objects = new object[] { new object(), new object(), new object() };

        Assert.AreEqual(0, array.Obtain(objects[0]));
        Assert.AreEqual(1, array.Obtain(objects[1]));
        Assert.AreEqual(2, array.Obtain(objects[2]));

        array.Release(1);
        array.Obtain(objects[1]);

        Assert.AreEqual(0, array.Available);

        var data = CollectAllData<object>(array);

        Assert.IsTrue(data.Count == 3);
        foreach( var obj in objects)
        {
            Assert.Contains(obj, data);
        }
    }

    private List<T> CollectAllData<T>(SlotArray<T> array) where T : class
    {
        var data = new List<T>();
        for (var idx = array.First; idx != -1; idx = array.Next(idx))
        {
            data.Add(array[idx]);
        }
        return data;
    }

    private void ObtainAll<T>(SlotArray<T> array, T[] values) where T : class
    {
        for (var i = 0; i < values.Length; i++)
        {
            var handle = array.Obtain(values[i]);
            
            Assert.IsTrue(handle != -1);
            Assert.IsTrue(array[handle] == values[i]);
        }
    }

    private void ReleaseAllInRandomOrder<T>(SlotArray<T> array) where T : class
    {
        var idxArray = Enumerations.CreateArray(array.Capacity, (idx) => idx)
                            .OrderBy(v => Random.Range(0, array.Capacity))
                            .ToArray();

        for (var i = 0; i < idxArray.Length; i++)
        {
            array.Release(idxArray[i]);
        }
    }

    private void ObtainAndReleaseAll<T>(SlotArray<T> array, T[] values, int repeat) where T : class
    {
        for (var i = 0; i < repeat; i++)
        {
            ObtainAll(array, values);
            
            Assert.IsTrue(array.Available == 0);
            var data = CollectAllData<T>(array);

            Assert.IsTrue(data.Count == values.Length);
            foreach (var obj in values)
            {
                Assert.Contains(obj, data);
            }

            ReleaseAllInRandomOrder(array);

            Assert.IsTrue(array.Available == values.Length);
            Assert.IsTrue(array.First == -1);
            for (var j = 0; j < array.Capacity; j++)
            {
                Assert.IsTrue(array[j] == null);
            }
        }
    }

    [Test]
    public void TestObtainAndReleaseAllOnce()
    {
        var objCount = 5;
        ObtainAndReleaseAll<object>(new SlotArray<object>(objCount), Enumerations.CreateArray<object>(objCount), 1);
    }

    [Test]
    public void TestObtainAndReleaseAllMultiple()
    {
        Random.InitState(42);
        var objCount = 10;
        ObtainAndReleaseAll<object>(new SlotArray<object>(objCount), Enumerations.CreateArray<object>(objCount), 100);
    }
}
