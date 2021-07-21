
using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using BareBones.Common;
using BareBones.Services.ObjectPool;

public class ObjectPoolCollectionTest
{
    private ObjectPoolCollection CreatePoolCollection(int collectionCount, int poolSize, string prefabName = "pooled prefab")
    {
        var obj = new GameObject();

        var collectionBehaviour = obj.AddComponent<ObjectPoolCollection>();
        var maxId = Enum.GetValues(typeof(PoolIdEnum)).Length;

        collectionBehaviour.poolCollectionConfig = new ObjectPoolConfig[collectionCount];

        for (var i = 0; i < collectionCount; i++)
        {
            collectionBehaviour.poolCollectionConfig[i] =
                new ObjectPoolConfig()
                {
                    _name = "pool(" + i + ")",
                    _prefab = new GameObject()
                    {
                        name = prefabName
                    },
                    preferredId = (PoolIdEnum)(i % maxId),
                    _size = poolSize
                };
        }

        return collectionBehaviour;
    }

    [Test]
    [Description("Awake an object pool collection and check if a collection contains a single pool.")]
    public void AwakeTest()
    {
        var locator = ResourceLocator._instance;
        ObjectPoolCollection behaviour = null;

        try
        {
            var prefabName = "pooled prefab";
            behaviour = CreatePoolCollection(1, 1, prefabName);

            behaviour.Awake();

#pragma warning disable CS0252
            Assert.IsTrue(locator.Resolve<IObjectPoolCollection<GameObject>>() == behaviour);
#pragma warning restore CS0252

            Assert.IsTrue(behaviour.PoolCount == 1);
            Assert.IsTrue(behaviour[0].PoolId == 0);
            Assert.IsTrue(behaviour[0].Capacity == 1);

            Assert.IsTrue(behaviour.transform.childCount == 1);
            Assert.IsTrue(behaviour.transform.GetChild(0).name == behaviour.poolCollectionConfig[0]._name);

            var poolObject = behaviour.transform.GetChild(0).gameObject;

            Assert.IsTrue(poolObject.transform.childCount == 1);
            Assert.IsTrue(poolObject.transform.GetChild(0).gameObject.activeInHierarchy == false);
            Assert.IsTrue(poolObject.transform.GetChild(0).name.IndexOf(prefabName) >= 0);
        }
        finally {
            if (behaviour != null)
            {
                locator.Deregister<IObjectPoolCollection<GameObject>>(behaviour);
            }
        }

    }

    [Test]
    [Description("Obtain and release an object from a pool in a poolCollection.")]
    public void ObtainAndReleaseTest()
    {
        var locator = ResourceLocator._instance;
        ObjectPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            var handle1 = collection.Obtain(0);
            var handle2 = collection.Obtain(0);
            var handle3 = collection.Obtain(0);

            Assert.IsTrue(handle1.HasReference);
            Assert.IsTrue(handle2.HasReference);
            Assert.IsTrue(!handle3.HasReference);

            var gameObject1 = collection.Dereference(handle1);
            var gameObject2 = collection.Dereference(handle2);

            Assert.IsTrue(gameObject1.activeInHierarchy);
            Assert.IsTrue(gameObject2.activeInHierarchy);
          
            Assert.IsTrue(collection.GetAvailable(0) == 0);
            Assert.IsTrue(collection.GetAvailable(1) == 2);

            collection.Release(handle1);
            collection.Release(handle2);

            Assert.IsTrue(collection.GetAvailable(0) == 2);
            Assert.IsTrue(collection.GetAvailable(1) == 2);

            Assert.IsFalse(gameObject1.activeInHierarchy);
            Assert.IsFalse(gameObject2.activeInHierarchy);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjectPoolCollection<GameObject>>(collection);
            }
        }
    }

    [Test]
    [Description("Remove a pool from the poolCollection.")]
    public void RemovePoolTest()
    {
        var locator = ResourceLocator._instance;
        ObjectPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            Assert.IsTrue(collection.transform.childCount == 2);

            var handle1 = collection.Obtain(0);
            var handle2 = collection.Obtain(0);

            var gameObject1 = collection.Dereference(handle1);
            var gameObject2 = collection.Dereference(handle2);

            collection.RemovePool(0);

            Assert.IsTrue(collection.PoolCount == 1);
            Assert.IsTrue(collection.transform.childCount == 1);
            Assert.IsTrue(gameObject1 == null);
            Assert.IsTrue(gameObject2 == null);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjectPoolCollection<GameObject>>(collection);
            }
        }
    }

    [Test]
    [Description("Remove a pool from the poolCollection but do not destroy the gameobjects in that pool.")]
    public void RemovePoolWithoutDestroyingGameObjectsTest()
    {
        var locator = ResourceLocator._instance;
        ObjectPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            Assert.IsTrue(collection.transform.childCount == 2);

            var handle1 = collection.Obtain(0);
            var handle2 = collection.Obtain(0);

            var gameObject1 = collection.Dereference(handle1);
            var gameObject2 = collection.Dereference(handle2);

            // remove the pool without destroying the gameobjects
            collection.RemovePool(0, false);

            Assert.IsTrue(collection.PoolCount == 1);
            Assert.IsTrue(collection.transform.childCount == 1);
            Assert.IsTrue(gameObject1 != null);
            Assert.IsTrue(gameObject2 != null);
        }
        finally
        {
            if (collection != null)
            {
                locator.Deregister<IObjectPoolCollection<GameObject>>(collection);
            }
        }
    }

    [Test]
    [Description("Adding a pool with the same id should be ignored.")]
    public void AddDuplicateIdTest()
    {
        var locator = ResourceLocator._instance;
        ObjectPoolCollection collection = null;

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
                locator.Deregister<IObjectPoolCollection<GameObject>>(collection);
            }
        }
    }

    [Test]
    [Description("Test whether or not updating triggers a sweep.")]
    public void UpdateAndSweepTest()
    {
        var locator = ResourceLocator._instance;
        ObjectPoolCollection collection = null;

        try
        {
            collection = CreatePoolCollection(2, 2);
            collection.Awake();

            var obj1 = collection.Dereference(collection.Obtain(0));
            var obj2 = collection.Dereference(collection.Obtain(1));

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
                locator.Deregister<IObjectPoolCollection<GameObject>>(collection);
            }
        }
    }
}
