using System;
using TMPro;
using UnityEngine;

public class PlayerDragUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private Player player;

    private void Start()
    {
       player.PlayerDragController.OnDragStart += PlayerDrag_OnDragStart;
    }

    private void PlayerDrag_OnDragStart()
    {
        Debug.Log("Drag Start");
    }
}
