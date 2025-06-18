using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnUI : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject player1Turn;
    [SerializeField] private GameObject player2Turn;
    [SerializeField] private GameObject player1You;
    [SerializeField] private GameObject player2You;
    [Space(5)]
    [SerializeField] private Image player1ImageBar;
    [SerializeField] private Image player1BackgroundBar;
    [Space(2.5f)]
    [SerializeField] private Image player2ImageBar;
    [SerializeField] private Image player2BackgroundBar;
    [Space(2.5f)]
    [SerializeField] private Sprite redBar;
    [SerializeField] private Sprite redBackgroundBar;
    [SerializeField] private Sprite greenBar;
    [SerializeField] private Sprite greenBackgroundBar;

    private BaseTurnManager turnManager;

    private void Start()
    {
        HideAllTurns();
        

        turnManager = ServiceLocator.Get<BaseTurnManager>();

        turnManager.OnLocalPlayableStateChanged += GameFlowManager_OnLocalPlayableStateChanged;

        turnManager.CurrentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;
        
        GameFlowManager_OnLocalPlayableStateChanged(); //check at start
        CurrentPlayableState_OnValueChanged(PlayableState.None, turnManager.CurrentPlayableState.Value); //check at start

    }

    private void GameFlowManager_OnLocalPlayableStateChanged()
    {
        if (turnManager.LocalPlayableState == PlayableState.Player1Playing)
        {
            player2You.SetActive(false);
            player1ImageBar.sprite = greenBar;
            player1BackgroundBar.sprite = greenBackgroundBar;
            player2ImageBar.sprite = redBar;
            player2BackgroundBar.sprite = redBackgroundBar;
        }
        else if (turnManager.LocalPlayableState == PlayableState.Player2Playing)
        {
            player1You.SetActive(false);
            player2ImageBar.sprite = greenBar;
            player2BackgroundBar.sprite = greenBackgroundBar;
            player1ImageBar.sprite = redBar;
            player1BackgroundBar.sprite = redBackgroundBar;
        }
    }

    private void CurrentPlayableState_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(newValue == PlayableState.Player1Playing)
        {
            ShowPlayer1Turn();
        } else if(newValue == PlayableState.Player2Playing)
        {
            ShowPlayer2Turn();
        }
    }

    private void HideAllTurns()
    {
        player1Turn.SetActive(false);
        player2Turn.SetActive(false);
    }

    private void ShowPlayer1Turn()
    {
        player1Turn.SetActive(true);
        player2Turn.SetActive(false);
    }

    private void ShowPlayer2Turn()
    {
        player2Turn.SetActive(true);
        player1Turn.SetActive(false);
    }

    private void OnDestroy()
    {
        turnManager.OnLocalPlayableStateChanged -= GameFlowManager_OnLocalPlayableStateChanged;

        turnManager.CurrentPlayableState.OnValueChanged -= CurrentPlayableState_OnValueChanged;
    }

}
