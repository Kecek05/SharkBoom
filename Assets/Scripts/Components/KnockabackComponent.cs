using UnityEngine;

public class KnockabackComponent : MonoBehaviour, IRecieveKnockback
{
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;

    public void DoOnRecieveKnockback(Vector3 knockback, Vector3 hitPos)
    {
        playerRagdollEnabler.TriggerRagdoll(knockback, hitPos);
    }
}
