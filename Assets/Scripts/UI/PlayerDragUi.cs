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
       player.PlayerDragController.OnDragDistanceChange += PlayerDrag_OnDragDistanceChange;
    }

    private void PlayerDrag_OnDragDistanceChange(float _force, Vector3 direction, float maxForceMultiplier)
    {
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        forcePercentage = (_force / maxForceMultiplier) * 100f; // change for max multiplier

        SetForceText(forcePercentage);
        SetAngleText(angle);
    }

    private void SetForceText(float _forcePercentage)
    {
        forceText.text = "Force: " + (int)forcePercentage;
    }

    private void SetAngleText(float _angle)
    {
        directionText.text = "Direction: " + _angle;
    }
}
