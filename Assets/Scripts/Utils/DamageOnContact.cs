using Unity.Netcode;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damage;
    public float Damage => damage;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerDamageControl playerDamageControl))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                playerDamageControl.CalculateDamage(damage);
                Debug.Log("Dealt " + damage + " damage to " + collision.gameObject.name);
            }
        }
        Destroy(gameObject);

    }

}
