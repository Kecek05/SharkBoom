using Unity.Netcode;
using UnityEngine;

public class TestingFieldNetworkStartup : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.StartHost();
    }
}
