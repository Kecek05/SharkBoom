using Sortify;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static event Action OnLoadMainMenu;


    private void Start()
    {
        OnLoadMainMenu?.Invoke();

        HostGameManager.OnFailToStartHost += HostGameManager_OnFailToStartHost;

    }

    private void HostGameManager_OnFailToStartHost()
    {
        //createGameBtn.interactable = true;
    }



    private void OnDestroy()
    {
        HostGameManager.OnFailToStartHost -= HostGameManager_OnFailToStartHost;
    }
}
