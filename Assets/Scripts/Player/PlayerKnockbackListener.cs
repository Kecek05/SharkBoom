using UnityEngine;

public class PlayerKnockbackListener : MonoBehaviour, IRecieveKnockback
{
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;

    public void DoOnRecieveKnockback(float knockbackStrength, Vector3 hitPos)
    {
        playerRagdollEnabler.TriggerRagdoll(knockbackStrength, hitPos);
        Debug.Log($"Player received knockback with strength: {knockbackStrength} at position: {hitPos}.");
    }
}
