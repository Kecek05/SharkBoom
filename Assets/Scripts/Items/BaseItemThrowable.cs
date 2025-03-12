using Sortify;
using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class BaseItemThrowable : MonoBehaviour
{

    public event Action OnItemFinishedAction;
    [BetterHeader("Base Item References")]
    [SerializeField] protected bool isServerObject;
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected CinemachineFollow cinemachineFollow;
    [SerializeField] protected GameObject[] collidersToChangeLayer;
    protected ItemLauncherData thisItemLaucherData;

    protected virtual void OnEnable()
    {
       // CameraManager.Instance.SetCameraState(CameraManager.CameraState.Following);
    }

    public virtual void Initialize(ItemLauncherData itemLauncherData)
    {
        thisItemLaucherData = itemLauncherData;

        switch(thisItemLaucherData.ownerPlayableState)
        {
            case PlayableState.Player1Playing:
                foreach(GameObject gameObject in collidersToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.PLAYER_1_LAYER;
                }
                break;
            case PlayableState.Player2Playing:
                foreach (GameObject gameObject in collidersToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.PLAYER_2_LAYER;
                }
                break;
        }

        ItemReleased(thisItemLaucherData.dragForce, thisItemLaucherData.dragDirection);
    }


    protected virtual void ItemReleased(float force, Vector3 direction)
    {
        CameraManager.Instance.CameraFollowing.SetTheValuesOfCinemachine(cinemachineFollow);

        rb.AddForce(direction * force, ForceMode.Impulse);

    }

    protected virtual void ItemCallbackAction()
    {
        if(!isServerObject) return; // Only the server should call the callback action

        GameFlowManager.Instance.PlayerPlayedRpc(thisItemLaucherData.ownerPlayableState);
    }

    protected void OnDestroy()
    {
        ItemCallbackAction();
    }
}
