using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using BareBones.Services.ObjectPool;
using BareBones.Game;
using System.Linq;

public class ObjectPoolTest
{
    [Test]
    [Description("Trivial test to see if absolute minimum of functionalities, the constructor, works")]
    public void ConstructorTest()
    {
        var count = 3;
        var objPool = new ObjectPool<object>(count);

        Assert.IsTrue(objPool.Available == count);
        Assert.IsTrue(objPool.Capacity == count);
    }

    [Test]
    [Description("Simple test to checking if obtaining a single object works")]
    public void ObtainSingleTest()
    {
        var obj = new object();
        var objPool = new ObjectPool<object>(obj);

        var result = objPool.Obtain();

        Assert.True(objPool.GetVersion(result.handle) == 1);
        Assert.True(objPool.GetSlotHandle(result.handle) == 0);
        Assert.True(result.obj == obj);

        Assert.IsTrue(objPool.Available == 0);
        Assert.IsTrue(objPool.Capacity == 1);
    }

    [Test]
    [Description("Simple test to checking if obtaining and releasing a single object works")]
    public void ObtainAndReleaseSingleTest()
    {
        var obj = new object();
        var objPool = new ObjectPool<object>(obj);

        var result = objPool.Obtain();

        objPool.Release(result.handle);

        Assert.IsTrue(objPool.Available == 1);

        result = objPool.Obtain();

        Assert.IsTrue(objPool.Available == 0);

        Assert.True(objPool.GetVersion(result.handle) == 3);
        Assert.True(objPool.GetSlotHandle(result.handle) == 0);
        Assert.True(result.obj == obj);
    }

    [Test]
    [Description("Tests if releasing a handle with an invalid version results in an assertion error")]
    public void ReleaseInvalidVersionTest()
    {
        var obj = new object();
        var objPool = new ObjectPool<object>(obj);

        objPool.Obtain();
        var illegalHandle = 2 << objPool.VersionShift;

        Assert.IsTrue(objPool.GetVersion(illegalHandle) == 2);

        LogAssert.Expect(UnityEngine.LogType.Error, new Regex(@"^ObjPool\.Release Fail.*$"));
        objPool.Release(illegalHandle);
    }


    private List<(int handle, T obj)> Obtain<T>(List<(int handle, T obj)> buffer, ObjectPool<T> pool, int count) where T : class
    {
        for (var i = 0; i < count && pool.Available > 0; i++)
        {
            buffer.Add(pool.Obtain());
        }

        return buffer;
    }

    private List<(int handle, T obj)> Release<T>(List<(int handle, T obj)> buffer, ObjectPool<T> pool, int count) where T : class
    {
        for (var i = 0; i < count && buffer.Count > 0; i++)
        {
            var idx = UnityEngine.Random.Range(0, buffer.Count);
            var handle = buffer[idx].handle;
            pool.Release(handle);
            buffer.RemoveAt(idx);
        }

        return buffer;
    }

    [Test]
    [Description("Monkey test grabbing and releasing all objects over several iterations.")]
    public void ObtainAndReleaseAllTest()
    {
        var count = 20;
        var objPool = new ObjectPool<object>(count);
        var buffer = new List<(int handle, object obj)>();

        UnityEngine.Random.InitState(42);

        for (var it = 0; it < 10; it++)
        {
            Obtain(buffer, objPool, count);

            Assert.IsTrue(buffer.Count == count);
            Assert.IsTrue(objPool.Available == 0);

            Release(buffer, objPool, count);

            Assert.IsTrue(buffer.Count == 0);
            Assert.IsTrue(objPool.Available == count);
        }
    }

    [Test]
    [Description("Obtain all and test if clear works.")]
    public void ClearTest()
    {
        var count = 20;
        var objPool = new ObjectPool<object>(count);

        UnityEngine.Random.InitState(42);

        for (var it = 0; it < 10; it++)
        {
            var obtainCount = UnityEngine.Random.Range(0, objPool.Available);
            for (var i = 0; i < obtainCount; i++)
            {
                objPool.Obtain();
            }

            objPool.Clear();

            Assert.IsTrue(objPool.Available == objPool.Capacity);
        }
    }

    [Test]
    [Description("Obtain and release with state=release to check the obj is not available immediately.")]
    public void ReleaseWithReleasedStateTest()
    {
        var objPool = new ObjectPool<object>(1);
        var objTuple = objPool.Obtain();

        objPool.Release(objTuple.handle, ObjectPoolState.Released);
        Assert.IsTrue(objPool.Available == 0);
        Assert.IsTrue(objPool.GetVersion(objTuple.handle) == 1);

        objPool.Release(objTuple.handle);
        Assert.IsTrue(objPool.Available == 1);
        Assert.IsTrue(objPool.GetVersion(objPool.Obtain().handle) == 3);
    }

    [Test]
    [Description("Monkey test grabbing and releasing some objects over several iterations.")]
    public void ObtainAndReleaseSomeTest()
    {
        var count = 120;
        var objPool = new ObjectPool<object>(count);
        var buffer = new List<(int handle, object obj)>();

        UnityEngine.Random.InitState(42);

        for (var it = 0; it < 48; it++)
        {
            Obtain(buffer, objPool, UnityEngine.Random.Range(0, objPool.Available));
            Release(buffer, objPool, UnityEngine.Random.Range(0, buffer.Count));
        }
    }

    // note that for a lot of iterations, it appears (provided the test is representative)
    // that the pool is a factor x40 to x50 faster. However for a lot of objects
    // with fewer iterations, the performance boost of the pool is only +/- 20% better.
    [Test]
    [Description("Speed test, showing how much faster/slower an object pool is.")]
    public void SpeedTest()
    {
        var gameObject = new GameObject();

        // add some random components
        gameObject.AddComponent<Hitpoints>();
        gameObject.AddComponent<LinearMovement>();
        gameObject.AddComponent<FollowTarget>();

        var objectCount = 20;
        var iterationCount = 250;

        var objPool = new ObjectPool<GameObject>(objectCount, (idx) => GameObject.Instantiate(gameObject));
        var handleBuffer = new List<(int handle, GameObject obj)>();
        var gameObjectBuffer = new List<GameObject>();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (var it = 0; it < iterationCount; it++)
        {
            while (objPool.Available > 0)
            {
                handleBuffer.Add(objPool.Obtain());
            }

            var i = 0;
            while (objPool.Available < objPool.Capacity)
            {
                objPool.Release(handleBuffer[i].handle);
                i++;
            }

            handleBuffer.Clear();
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("pool ms = " + stopwatch.ElapsedMilliseconds);

        stopwatch.Reset();
        stopwatch.Start();
        for (var it = 0; it < iterationCount; it++)
        {
            while (gameObjectBuffer.Count < objectCount)
            {
                gameObjectBuffer.Add(GameObject.Instantiate(gameObject));
            }

            var i = 0;
            while (i < gameObjectBuffer.Count)
            {
                GameObject.DestroyImmediate(gameObjectBuffer[i]);
                i++;
            }

            gameObjectBuffer.Clear();
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("instantiate/destroy ms = " + stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Description("Check if version looping works as expected.")]
    public void VersionLoopTest()
    {
        var objPool = new ObjectPool<object>(1)
        {
            VersionMask = 0xff,
            VersionShift = 8
        };

        for (var it = 0; it < 128; it++)
        {
            var (handle, obj) = objPool.Obtain();
            objPool.Release(handle);
            Assert.IsTrue((it * 2) + 1 == objPool.GetVersion(handle));
        }

        var version = objPool.GetVersion((objPool.Obtain().handle));
        Assert.IsTrue(version == 1);
    }

    [Test]
    [Description("Check if sweep releases up objects in stages .")]
    public void SweepTest()
    {
        var objPool = new ObjectPool<GameObject>(2, (idx) =>
        {
            var result = new GameObject();
            result.SetActive(true);
            return result;
        });

        var obj1Ref = objPool.Obtain();
        var obj2Ref = objPool.Obtain();

        objPool.Sweep((g) => !g.activeSelf);

        Assert.IsTrue(objPool.Available == 0);
        Assert.IsTrue(objPool.GetState(obj1Ref.handle) == ObjectPoolState.InUse);
        Assert.IsTrue(objPool.GetState(obj2Ref.handle) == ObjectPoolState.InUse);

        obj1Ref.obj.SetActive(false);

        objPool.Sweep((g) => !g.activeSelf);

        Assert.IsTrue(objPool.Available == 0);
        Assert.IsTrue(objPool.GetState(obj1Ref.handle) == ObjectPoolState.Released);
        Assert.IsTrue(objPool.GetState(obj2Ref.handle) == ObjectPoolState.InUse);

        objPool.Sweep((g) => !g.activeSelf);

        Assert.IsTrue(objPool.Available == 1);
        Assert.IsTrue(objPool.GetState(obj1Ref.handle) == ObjectPoolState.Available);
        Assert.IsTrue(objPool.GetState(obj2Ref.handle) == ObjectPoolState.InUse);
    }

    [Test]
    [Description("Check if the objectpool can enumerate over all objects in use.")]
    public void EnumerationTest()
    {
        var objPool = new ObjectPool<GameObject>(3, (idx) =>
        {
            var result = new GameObject();
            result.SetActive(true);
            return result;
        });

        var obj1Ref = objPool.Obtain();
        var obj2Ref = objPool.Obtain();

        objPool.All(obj => obj == obj1Ref.obj || obj == obj2Ref.obj);
    }
}