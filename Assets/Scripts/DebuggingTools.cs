using QFSW.QC;
using Sortify;
using Unity.Netcode;
using UnityEngine;

public class DebuggingTools : MonoBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private GameObject projectilePrefab;


    [Command("debug-spawnProjectile")]
    public void SpawnProjectile()
    {
        SpawnProjectileRpc();
    }

    [Rpc(SendTo.Server)]
    private void SpawnProjectileRpc()
    {
        SpawnProjectileClientsRpc();
        Debug.Log("Server");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnProjectileClientsRpc()
    {
        GameObject projectileSpawned = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);

        Debug.Log("Projectile spawned");
    }
}
