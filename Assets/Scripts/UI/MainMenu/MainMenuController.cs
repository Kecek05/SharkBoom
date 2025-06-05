using Sortify;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;
    public static event Action OnLoadMainMenu;


    private void Start()
    {
        OnLoadMainMenu?.Invoke();

        HostGameManager.OnFailToStartHost += HostGameManager_OnFailToStartHost;

    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
        Debug.Log("ButtonWorking");
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
        Debug.Log("ButtonWorking");
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
        Debug.Log("ButtonWorking");
    }
    public void HideSettings()
    {
        settingsPanel.SetActive(false);
        Debug.Log("ButtonWorking");
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
