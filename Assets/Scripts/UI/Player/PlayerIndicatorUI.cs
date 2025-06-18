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

    public void InitializeOwner()
    {
        ShowPlayerIndicator();
    }

    private void HidePlayerIndicator()
    {
        playerIndicator.SetActive(false);
    }

    private void ShowPlayerIndicator()
    {
        playerIndicator.SetActive(true);
    }

    public void UnitializeOwner()
    {
        HidePlayerIndicator();
    }
}
