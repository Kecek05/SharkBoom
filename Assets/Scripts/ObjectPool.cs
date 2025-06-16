using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

[Serializable]
struct PoolConfigObject
{
    public ItemSO itemSO;
    public int PrewarmCount;
}

public class ObjectPool : MonoBehaviour
{

    public static ObjectPool Instance { get; private set; }

    [SerializeField] private List<PoolConfigObject> pooledPrefabsList;
    
    HashSet<ItemSO> m_ItemsSO = new HashSet<ItemSO>();

    Dictionary<int, ObjectPool<GameObject>> m_PooledObjects = new Dictionary<int, ObjectPool<GameObject>>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        foreach (var configObject in pooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.itemSO, configObject.PrewarmCount);
        }
    }

    private void OnDisable()
    {
        // Unregisters all objects in PooledPrefabsList from the cache.
        foreach (var itemSO in m_ItemsSO)
        {
            m_PooledObjects[itemSO.itemID].Clear();
        }
        m_PooledObjects.Clear();
        m_ItemsSO.Clear();
    }

    /// <summary>
    /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
    /// </summary>
    public GameObject GetObject(int itemID, Vector3 position, Quaternion rotation)
    {   
        var objectPooled = m_PooledObjects[itemID].Get();

        var objectTranform = objectPooled.transform;
        objectTranform.position = position;
        objectTranform.rotation = rotation;

        return objectPooled;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    public void ReturnObject(GameObject gameObject, int itemID)
    {
        Debug.Log($"POOL - Returning object of type {gameObject.name} to pool with ID {itemID}");
        m_PooledObjects[itemID].Release(gameObject);
    }

    /// <summary>
    /// Builds up the cache for a prefab.
    /// </summary>
    void RegisterPrefabInternal(ItemSO itemSO, int prewarmCount)
    {
        GameObject CreateFunc()
        {
            return Instantiate(itemSO.itemPrefab).gameObject;
        }

        void ActionOnGet(GameObject gameObject)
        {
            gameObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(GameObject gameObject)
        {
            gameObject.gameObject.SetActive(false);
        }

        void ActionOnDestroy(GameObject gameObject)
        {
            Destroy(gameObject.gameObject);
        }

        m_ItemsSO.Add(itemSO);

        // Create the pool
        m_PooledObjects[itemSO.itemID] = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

        // Populate the pool
        var prewarmObjects = new List<GameObject>();
        for (var i = 0; i < prewarmCount; i++)
        {
            prewarmObjects.Add(m_PooledObjects[itemSO.itemID].Get());
        }
        foreach (var networkObject in prewarmObjects)
        {
            m_PooledObjects[itemSO.itemID].Release(networkObject);
        }

    }
}



