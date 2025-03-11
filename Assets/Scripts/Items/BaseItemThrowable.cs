using Sortify;
using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class BaseItemThrowable : MonoBehaviour, IDraggable
{

    public event Action OnItemFinishedAction;
    [BetterHeader("Base Item References")]
    [SerializeField] protected bool isServerObject;
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected CinemachineFollow cinemachineFollow;
    [SerializeField] protected GameObject[] collidersToChangeLayer;
    protected GameFlowManager.PlayableState ownerPlayableState;

    protected virtual void OnEnable()
    {
        CameraManager.Instance.SetCameraState(CameraManager.CameraState.Following);
    }

    public void Initialize(GameFlowManager.PlayableState _ownerPlayableState)
    {
        ownerPlayableState = _ownerPlayableState;

        switch(ownerPlayableState)
        {
            case GameFlowManager.PlayableState.Player1Playing:
                foreach(GameObject gameObject in collidersToChangeLayer)
                {
                    gameObject.layer = GameFlowManager.PLAYER_1_LAYER;
                }
                break;
            case GameFlowManager.PlayableState.Player2Playing:
                foreach (GameObject gameObject in collidersToChangeLayer)
                {
                    gameObject.layer = GameFlowManager.PLAYER_2_LAYER;
                }
                break;
        }

    }

    public void Release(float force, Vector3 direction)
    {
        
        CameraManager.Instance.CameraFollowing.SetTheValuesOfCinemachine(cinemachineFollow);
        ItemReleased(force, direction);
        
    }

    protected virtual void ItemReleased(float force, Vector3 direction)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
        Debug.Log($"Released: {gameObject.name} Force: {force} Direction: {direction}");
    }

    protected virtual void ItemCallbackAction()
    {
        if(!isServerObject) return; // Only the server should call the callback action

        GameFlowManager.Instance.PlayerPlayedRpc(ownerPlayableState);

        //GameFlowManager.Instance.ItemFinishOurAction();
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!isServerObject)
        {
            //Change latter
            Destroy(gameObject); //Client doesnt do damage
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
        Destroy(gameObject);
    }

    protected void OnDestroy()
    {
        ItemCallbackAction();
    }
}
