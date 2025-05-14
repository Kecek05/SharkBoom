using UnityEngine;

public class KnockbackListenerNetworkedComponent : MonoBehaviour, IRecieveKnockback
{
    public void DoOnRecieveKnockback(Vector3 knockback, Vector3 hitPos, Rigidbody rigidbody)
    {
        //playerRagdollEnabler.TriggerRagdoll(knockback, hitPos);

        rigidbody.AddForceAtPosition(knockback, hitPos, ForceMode.Impulse);
    }
}
