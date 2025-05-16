using UnityEngine;

public class PlayerKnockbackListenerNetworked : MonoBehaviour, IRecieveKnockback
{
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;

    public void DoOnRecieveKnockback(float knockbackStrength, Vector3 hitPos)
    {
        playerRagdollEnabler.TriggerRagdoll(knockbackStrength, hitPos);
    }
}
