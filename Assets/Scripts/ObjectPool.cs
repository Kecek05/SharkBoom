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
    /*public override void OnNetworkSpawn()
    {
        // Registers all objects in PooledPrefabsList to the cache.
        foreach (var configObject in pooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.itemSO, configObject.PrewarmCount);
        }
    }*/

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

   /* public override void OnNetworkDespawn()
    {
        // Unregisters all objects in PooledPrefabsList from the cache.
        foreach (var itemSO in m_ItemsSO)
        {
            // Unregister Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(itemSO.itemPrefab);
            m_PooledObjects[itemSO.itemIndex].Clear();
        }
        m_PooledObjects.Clear();
        m_ItemsSO.Clear();
    }*/

   /* public void OnValidate()
    {
        for (var i = 0; i < pooledPrefabsList.Count; i++)
        {
            var prefab = pooledPrefabsList[i].itemSO.itemPrefab;
            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(ObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }*/


    /// <summary>
    /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
    /// </summary>
    /// <remarks>
    /// To spawn a NetworkObject from one of the pools, this must be called on the server, then the instance
    /// returned from it must be spawned on the server. This method will then also be called on the client by the
    /// PooledPrefabInstanceHandler when the client receives a spawn message for a prefab that has been registered
    /// here.
    /// </remarks>
    /// <param name="prefab"></param>
    /// <param name="position">The position to spawn the object at.</param>
    /// <param name="rotation">The rotation to spawn the object with.</param>
    /// <returns></returns>
    public GameObject GetObject(int itemIndex, Vector3 position, Quaternion rotation)
    {
        var objectPooled = m_PooledObjects[itemIndex].Get();

        var objectTranform = objectPooled.transform;
        objectTranform.position = position;
        objectTranform.rotation = rotation;

        return objectPooled;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    public void ReturnObject(GameObject gameObject, int itemIndex)
    {
        m_PooledObjects[itemIndex].Release(gameObject);
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

        // Register Netcode Spawn handlers
       // NetworkManager.Singleton.PrefabHandler.AddHandler(itemSO.itemPrefab, new PooledPrefabInstanceHandler(itemSO.itemIndex, this));
    }

    /*class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        int m_PrefabIndex;
        ObjectPool m_Pool;

        public PooledPrefabInstanceHandler(int prefabIndex, ObjectPool pool)
        {
            m_PrefabIndex = prefabIndex;
            m_Pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return m_Pool.GetObject(m_PrefabIndex, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnObject(networkObject, m_PrefabIndex);
        }
    }*/

}



