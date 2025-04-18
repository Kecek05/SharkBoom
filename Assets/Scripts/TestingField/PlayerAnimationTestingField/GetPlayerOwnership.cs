using Unity.Netcode;
using UnityEngine;

public class GetPlayerOwnership : MonoBehaviour
{

    [SerializeField] private NetworkObject playerNetworkObject;

    private void Start()
    {
        NetworkManager.Singleton.StartHost();

        playerNetworkObject.ChangeOwnership(0);
    }

}
