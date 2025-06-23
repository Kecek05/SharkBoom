using System;
using Unity.Netcode;
using UnityEngine;

public class LeaveLobbyMeny : MonoBehaviour
{
    [SerializeField] private GameObject leaveLobby;
    
    private void Start()
    {
        if(NetworkManager.Singleton.IsHost)
            leaveLobby.SetActive(true);
    }
}
