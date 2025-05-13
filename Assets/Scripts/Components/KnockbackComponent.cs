using UnityEngine;

public class KnockbackComponent : MonoBehaviour, IRecieveKnockback
{
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;

    public void DoOnRecieveKnockback(Vector3 knockback, Vector3 hitPos)
    {
        playerRagdollEnabler.TriggerRagdoll(knockback, hitPos);
    }
}
