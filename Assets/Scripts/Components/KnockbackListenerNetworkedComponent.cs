using UnityEngine;

public class KnockbackListenerNetworkedComponent : MonoBehaviour, IRecieveKnockback
{
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;
    public void DoOnRecieveKnockback(Vector3 knockback, Vector3 hitPos)
    {
        playerRagdollEnabler.TriggerRagdoll(knockback, hitPos);
    }
}
