using Sortify;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class BaseItemThrowable : MonoBehaviour, IDraggable
{
    [BetterHeader("References")]
    [SerializeField] protected bool isServerObject;
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected CinemachineFollow cinemachineFollow;
    protected Transform shooterTransform;

    private void OnEnable()
    {
        CameraManager.Instance.SetCameraState(CameraManager.CameraState.Following);
    }

    public void Release(float force, Vector3 direction, Transform _shooterTransform)
    {
        shooterTransform = _shooterTransform;
        
        CameraManager.Instance.CameraFollowing.SetTheValuesOfCinemachine(cinemachineFollow);
        ItemReleased(force, direction);
        
    }

    protected virtual void ItemReleased(float force, Vector3 direction)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
        
       
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!isServerObject)
        {
            //Change latter
            Destroy(gameObject);
            return;
        }

        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                damageable.TakeDamage(itemSO.damageableSO);
                Debug.Log("Dealt " + itemSO.damageableSO.damage + " damage to " + collision.gameObject.name);
            }
        }

    }
}
