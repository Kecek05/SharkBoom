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
        GameObject projectileSpawned = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);

        projectileSpawned.GetComponent<NetworkObject>().Spawn(true);

        Debug.Log("Projectile spawned");
    }
}
