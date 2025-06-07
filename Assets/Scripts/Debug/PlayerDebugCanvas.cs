using System;
using Sortify;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerDebugCanvas : NetworkBehaviour
{

    [BetterHeader("References")]
    public PlayerRagdollEnabler playerRagdollEnabler;
    public TextMeshProUGUI hitedDebugText;

    private void Update()
    {
        if (playerRagdollEnabler.hitedRbDebug)
        {
            hitedDebugText.text = $"Hited Rigidbody: {playerRagdollEnabler.hitedRbDebug.name} - " +
                                  $"Velocity: {playerRagdollEnabler.hitedRbDebug.linearVelocity.magnitude:F2} m/s";
        }
        else
        {
            hitedDebugText.text = "No Rigidbody hit detected.";
        }
    }
}
