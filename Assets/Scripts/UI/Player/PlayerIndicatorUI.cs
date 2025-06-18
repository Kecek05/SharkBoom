using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class PlayerIndicatorUI : NetworkBehaviour
{ 
    [SerializeField] private GameObject playerIndicator;

    public override void OnNetworkSpawn()
    {
        HidePlayerIndicator();
    }

    public void HidePlayerIndicator()
    {
        playerIndicator.SetActive(false);
    }

    public void ShowPlayerIndicator()
    {
        playerIndicator.SetActive(true);
    }
}
