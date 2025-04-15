using UnityEngine;

public class PlayerRagdollEnabler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private bool startRagdoll = false;

    public Rigidbody[] ragdollRbs;
    public CharacterJoint[] ragdollJoints;
    public Collider[] ragdollColliders;

    private void Awake()
    {
        ragdollRbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        ragdollJoints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        ragdollColliders = ragdollRoot.GetComponentsInChildren<Collider>();

        if (startRagdoll)
        {
            EnableRagdoll();
        }
        else
        {
            EnableAnimator();
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            startRagdoll = !startRagdoll;

            if (startRagdoll)
            {
                EnableRagdoll();
            }
            else
            {
                EnableAnimator();
            }
        }
    }

    private void EnableRagdoll()
    {
        animator.enabled = false;

        foreach (CharacterJoint characterJoint in ragdollJoints)
        {
            characterJoint.enableCollision = true;
        }

        foreach (Collider ragColliders in ragdollColliders)
        {
            ragColliders.enabled = true;
        }

        foreach (Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.detectCollisions = true;
            ragdollRb.useGravity = true;
        }

    }

    private void EnableAnimator()
    {
        animator.enabled = true;

        foreach (CharacterJoint characterJoint in ragdollJoints)
        {
            characterJoint.enableCollision = false;
        }

        foreach(Collider ragColliders in  ragdollColliders)
        {
            ragColliders.enabled = false;
        }

        foreach(Rigidbody ragdollRb in ragdollRbs)
        {
            ragdollRb.detectCollisions = false;
            ragdollRb.useGravity = false;
        }
    }

}
