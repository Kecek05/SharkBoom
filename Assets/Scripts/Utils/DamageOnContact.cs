using Unity.Netcode;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damage;
    public float Damage => damage;
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.rigidbody != null)
        {
            if(collision.rigidbody.TryGetComponent(out IDamageable damageableObject))
            {
                if(NetworkManager.Singleton.IsServer)
                {
                    damageableObject.TakeDamage(damage);
                    Debug.Log("Dealt " + damage + " damage to " + collision.gameObject.name);
                } else if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("IsHost");
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    Debug.Log("IsClient");
                }
                Destroy(gameObject);
                //DestroyRpc();
            }
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    [Rpc(SendTo.Server)]
    private void DestroyRpc()
    {
        Debug.Log("Destroying object on server");
        gameObject.transform.GetComponent<NetworkObject>().Despawn(true);
        //Destroy(gameObject);
    }

}
