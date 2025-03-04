using QFSW.QC;
using Sortify;
using Unity.Netcode;
using UnityEngine;

public class DebuggingTools : NetworkBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnProjectilePos;

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
        GameObject projectileSpawned = Instantiate(projectilePrefab, spawnProjectilePos.position, Quaternion.identity);

        Debug.Log("Projectile spawned");
    }
}
