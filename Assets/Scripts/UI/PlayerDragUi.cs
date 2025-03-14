using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDragUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private Player player;

    private float angle;
    private float forcePercentage;

    private void Start()
    {
       player.PlayerDragController.OnDragChange += PlayerDrag_OnDragChange;
       
    }

    private void PlayerDrag_OnDragChange()
    {
        SetForceText();
        SetAngleText();
    }

    private void SetForceText()
    {
        forceText.text = "Force: " + Mathf.RoundToInt(player.PlayerDragController.GetForcePercentage());
    }

    private void SetAngleText()
    {
        directionText.text = "Direction: " + Mathf.RoundToInt(player.PlayerDragController.GetAngle());
    }

    private void ShowText()
    {
        forceText.enabled = true;
        directionText.enabled = true;
    }

    private void HideText()
    {
        forceText.enabled = false;
        directionText.enabled = false;
    }

}
