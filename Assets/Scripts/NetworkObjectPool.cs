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

public class NetworkObjectPool : NetworkBehaviour
{

    public static NetworkObjectPool Instance { get; private set; }

    [SerializeField] private List<PoolConfigObject> pooledPrefabsList;

    HashSet<ItemSO> m_ItemsSO = new HashSet<ItemSO>();

    Dictionary<int, ObjectPool<NetworkObject>> m_PooledObjects = new Dictionary<int, ObjectPool<NetworkObject>>();

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
    }
    public override void OnNetworkSpawn()
    {
        // Registers all objects in PooledPrefabsList to the cache.
        foreach (var configObject in pooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.itemSO, configObject.PrewarmCount);
        }
    }

    public override void OnNetworkDespawn()
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
    }

    public void OnValidate()
    {
        for (var i = 0; i < pooledPrefabsList.Count; i++)
        {
            var prefab = pooledPrefabsList[i].itemSO.itemPrefab;
            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }


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
    public NetworkObject GetNetworkObject(int itemIndex, Vector3 position, Quaternion rotation)
    {
        var networkObject = m_PooledObjects[itemIndex].Get();

        var noTransform = networkObject.transform;
        noTransform.position = position;
        noTransform.rotation = rotation;

        return networkObject;
    }

    /// <summary>
    /// Return an object to the pool (reset objects before returning).
    /// </summary>
    public void ReturnNetworkObject(NetworkObject networkObject, int itemIndex)
    {
        m_PooledObjects[itemIndex].Release(networkObject);
    }

    /// <summary>
    /// Builds up the cache for a prefab.
    /// </summary>
    void RegisterPrefabInternal(ItemSO itemSO, int prewarmCount)
    {
        NetworkObject CreateFunc()
        {
            return Instantiate(itemSO.itemPrefab).GetComponent<NetworkObject>();
        }

        void ActionOnGet(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(false);
        }

        void ActionOnDestroy(NetworkObject networkObject)
        {
            Destroy(networkObject.gameObject);
        }

        m_ItemsSO.Add(itemSO);

        // Create the pool
        m_PooledObjects[itemSO.itemIndex] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

        // Populate the pool
        var prewarmNetworkObjects = new List<NetworkObject>();
        for (var i = 0; i < prewarmCount; i++)
        {
            prewarmNetworkObjects.Add(m_PooledObjects[itemSO.itemIndex].Get());
        }
        foreach (var networkObject in prewarmNetworkObjects)
        {
            m_PooledObjects[itemSO.itemIndex].Release(networkObject);
        }

        // Register Netcode Spawn handlers
        NetworkManager.Singleton.PrefabHandler.AddHandler(itemSO.itemPrefab, new PooledPrefabInstanceHandler(itemSO.itemIndex, this));
    }

    class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        int m_PrefabIndex;
        NetworkObjectPool m_Pool;

        public PooledPrefabInstanceHandler(int prefabIndex, NetworkObjectPool pool)
        {
            m_PrefabIndex = prefabIndex;
            m_Pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return m_Pool.GetNetworkObject(m_PrefabIndex, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnNetworkObject(networkObject, m_PrefabIndex);
        }
    }

}



