using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerTurnUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject player1Turn;
    [SerializeField] private GameObject player2Turn;
    [SerializeField] private GameObject player1You;
    [SerializeField] private GameObject player2You;



    private void Start()
    {
        HideAllTurns();

        GameFlowManager.OnLocalPlayableStateChanged += GameFlowManager_OnLocalPlayableStateChanged;
        GameFlowManager_OnLocalPlayableStateChanged(); //check at start

        GameFlowManager.Instance.CurrentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;
    }

    private void GameFlowManager_OnLocalPlayableStateChanged()
    {
        if (GameFlowManager.Instance.LocalplayableState == PlayableState.Player1Playing)
        {
            player2You.SetActive(false);
        }
        else if (GameFlowManager.Instance.LocalplayableState == PlayableState.Player2Playing)
        {
            player1You.SetActive(false);
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
        GameFlowManager.OnLocalPlayableStateChanged -= GameFlowManager_OnLocalPlayableStateChanged;

        GameFlowManager.Instance.CurrentPlayableState.OnValueChanged -= CurrentPlayableState_OnValueChanged;
    }

}
