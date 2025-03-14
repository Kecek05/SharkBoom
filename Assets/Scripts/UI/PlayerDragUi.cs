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
    private int forcePercentage;
    private int force;

    private void Start()
    {
       player.PlayerDragController.OnDragDistanceChange += PlayerDrag_OnDragDistanceChange;
    }

    private void PlayerDrag_OnDragDistanceChange(float _force, Vector3 direction)
    {
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        force = (int)_force;
        forcePercentage = (force / 50) * 100; // change for max multiplier

        SetForceText(forcePercentage);
        SetAngleText(angle);
    }

    private void SetForceText(int _force)
    {
        forceText.text = "Force: " + _force;
        Debug.Log("Force: " + _force);
    }

    private void SetAngleText(float _angle)
    {
        directionText.text = "Direction: " + _angle;
    }
}
