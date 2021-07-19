
using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using BareBones.Common;
using BareBones.Services.ObjectPool;

public class ObjectPoolCollectionTest
{
    private ObjPoolCollection CreatePoolCollection(int collectionCount, int poolSize, string prefabName = "pooled prefab")
    {
        var obj = new GameObject();

        var collectionBehaviour = obj.AddComponent<ObjPoolCollection>();
        var maxId = Enum.GetValues(typeof(PoolIdEnum)).Length;

        collectionBehaviour.poolCollectionConfig = new ObjectPoolConfig[collectionCount];

        for (var i = 0; i < collectionCount; i++)
        {
            collectionBehaviour.poolCollectionConfig[i] =
                new ObjectPoolConfig()
                {
                    name = "pool(" + i + ")",
                    prefab = new GameObject()
                    {
                        name = prefabName
                    },
                    preferredId = (PoolIdEnum)(i % maxId),
                    size = poolSize
                };
        }

        return collectionBehaviour;
    }

    [Test]
    [Description("Awake an object pool collection and check if a collection contains a single pool.")]
    public void AwakeTest()
    {
        var locator = ResourceLocator._instance;
        ObjPoolCollection behaviour = null;

        try
        {
            var prefabName = "pooled prefab";
            behaviour = CreatePoolCollection(1, 1, prefabName);

            behaviour.Awake();

#pragma warning disable CS0252
            Assert.IsTrue(locator.Resolve<IObjPoolCollection>() == behaviour);
#pragma warning restore CS0252

            Assert.IsTrue(behaviour.PoolCount == 1);
            Assert.IsTrue(behaviour[0].PoolId == 0);
            Assert.IsTrue(behaviour[0].Capacity == 1);

            Assert.IsTrue(behaviour.transform.childCount == 1);
            Assert.IsTrue(behaviour.transform.GetChild(0).name == behaviour.poolCollectionConfig[0].name);

            var poolObject = behaviour.transform.GetChild(0).gameObject;

            Assert.IsTrue(poolObject.transform.childCount == 1);
            Assert.IsTrue(poolObject.transform.GetChild(0).gameObject.activeInHierarchy == false);
            Assert.IsTrue(poolObject.transform.GetChild(0).name.IndexOf(prefabName) >= 0);
        }
        finally {
            if (behaviour != null)
            {
                locator.Deregister<IObjPoolCollection>(behaviour);
            }
        }

    }

    [Test]
    [Description("Obtain and release an object from a pool in a poolCollection.")]
    public void ObtainAndReleaseTest()
    {
        var locator = ResourceLocator._instance;
        ObjPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            var handle1 = collection.Obtain(0);
            var handle2 = collection.Obtain(0);
            var handle3 = collection.Obtain(0);

            Assert.IsTrue(handle1.HasValue);
            Assert.IsTrue(handle2.HasValue);
            Assert.IsFalse(handle3.HasValue);

            Assert.IsTrue(handle1.Value.gameObject.activeInHierarchy);
            Assert.IsTrue(handle2.Value.gameObject.activeInHierarchy);

            Assert.IsTrue(collection.GetAvailable(0) == 0);
            Assert.IsTrue(collection.GetAvailable(1) == 2);

            collection.Release(handle1.Value);
            collection.Release(handle2.Value);

            Assert.IsTrue(collection.GetAvailable(0) == 2);
            Assert.IsTrue(collection.GetAvailable(1) == 2);

            Assert.IsFalse(handle1.Value.gameObject.activeInHierarchy);
            Assert.IsFalse(handle2.Value.gameObject.activeInHierarchy);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjPoolCollection>(collection);
            }
        }
    }

    [Test]
    [Description("Remove a pool from the poolCollection.")]
    public void RemovePoolTest()
    {
        var locator = ResourceLocator._instance;
        ObjPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            Assert.IsTrue(collection.transform.childCount == 2);

            var handle1 = collection.Obtain(0);
            var handle2 = collection.Obtain(0);

            collection.RemovePool(0);

            Assert.IsTrue(collection.PoolCount == 1);
            Assert.IsTrue(collection.transform.childCount == 1);
            Assert.IsTrue(handle1.Value.gameObject == null);
            Assert.IsTrue(handle2.Value.gameObject == null);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjPoolCollection>(collection);
            }
        }
    }

    [Test]
    [Description("Remove a pool from the poolCollection but do not destroy the gameobjects in that pool.")]
    public void RemovePoolWithoutDestroyingGameObjectsTest()
    {
        var locator = ResourceLocator._instance;
        ObjPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            Assert.IsTrue(collection.transform.childCount == 2);

            var handle1 = collection.Obtain(0);
            var handle2 = collection.Obtain(0);

            collection.RemovePool(0, false);

            Assert.IsTrue(collection.PoolCount == 1);
            Assert.IsTrue(collection.transform.childCount == 1);
            Assert.IsTrue(handle1.Value.gameObject != null);
            Assert.IsTrue(handle2.Value.gameObject != null);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjPoolCollection>(collection);
            }
        }
    }

    [Test]
    [Description("Adding a pool with the same id should be ignored.")]
    public void AddDuplicateIdTest()
    {
        var locator = ResourceLocator._instance;
        ObjPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(1, 1);
            collection.Awake();

            Assert.IsTrue(collection.transform.childCount == 1);

            LogAssert.Expect(LogType.Warning, new Regex("^Duplicate Id.*$"));
            collection.AddPool("pool", 0, 1, new GameObject());

            Assert.IsTrue(collection.PoolCount == 1);
            Assert.IsTrue(collection.transform.childCount == 1);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjPoolCollection>(collection);
            }
        }
    }

    [Test]
    [Description("Test whether or not updating triggers a sweep.")]
    public void UpdateAndSweepTest()
    {
        var locator = ResourceLocator._instance;
        ObjPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            var obj1 = collection.Obtain(0).Value.gameObject;
            var obj2 = collection.Obtain(1).Value.gameObject;

            collection.Update();

            Assert.IsTrue(collection.GetAvailable(0) == 1);
            Assert.IsTrue(collection.GetAvailable(1) == 1);

            // will put the game object meta in 'released state'
            // in which its inactive in the scene but not returned to the pool yet
            obj1.transform.parent = null;
            obj1.SetActive(false);

            collection.Update();

            Assert.IsTrue(obj1.transform.parent == null);
            Assert.IsTrue(collection.GetAvailable(0) == 1);
            Assert.IsTrue(collection.GetAvailable(1) == 1);

            // second update should return the handle to the pool
            collection.Update();

            Assert.IsTrue(obj1.transform.parent != null);
            Assert.IsTrue(collection.GetAvailable(0) == 2);
            Assert.IsTrue(collection.GetAvailable(1) == 1);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjPoolCollection>(collection);
            }
        }
    }
}
