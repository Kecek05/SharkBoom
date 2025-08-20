using System.Collections;
using UnityEngine;

/// <summary>
/// Component that rotates a rigidbody object to face its velocity direction.
/// Commonly used for projectiles like spears/harpoons to maintain realistic orientation during flight.
/// Includes jitter prevention through rotation threshold and immediate snap-to-target rotation.
/// </summary>
public class RotateTowardsVelocityComponent : BaseItemComponent
{
    [Tooltip("How fast the spear rotates to align with its velocity.")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float velocityThreshold = 4f; // Minimum velocity to consider for rotation
    [Tooltip("Minimum angle difference in degrees before rotation is updated (prevents jittering).")]
    [SerializeField] private float rotationThreshold = 10f; // Minimum angle difference to consider for rotation
    private WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    
    private Coroutine rotateCoroutine;
    private float previousTargetAngle = float.NaN; // Track the previous target angle

    protected override void OnEnableComponent()
    {
        //Rotate the object to the right direction that the player is facing and get the right rotation, if is positive or if is negative

        /*if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }*/

        // Reset previous angle tracking when component is enabled
        previousTargetAngle = float.NaN;

        if (rb.linearVelocity.sqrMagnitude > velocityThreshold)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); // Aplica imediatamente
            previousTargetAngle = angle; // Store initial angle
        }
    }

    protected override void DoComponentLogic()
    {
        rotateCoroutine ??= StartCoroutine(RotateObject()); //if not null start the coroutine and assign it to rotateCoroutine
    }

    private IEnumerator RotateObject()
    {
        yield return null;

        while (true)
        {
            Vector3 vel = rb.linearVelocity;
            if (vel.sqrMagnitude > velocityThreshold)
            {
                float targetAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                
                // Get current rotation angle
                float currentAngle = transform.rotation.eulerAngles.z;
                
                // Only rotate if this is the first time or if the angle difference is significant
                if (float.IsNaN(previousTargetAngle) || Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle)) > rotationThreshold)
                {
                    // Snap to target rotation immediately to prevent jittering from gradual Slerp
                    transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
                    previousTargetAngle = targetAngle;
                }

                //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            yield return WaitForFixedUpdate;
        }
        rotateCoroutine = null;
    }

    protected override void OnDisableComponent()
    {
        if(rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }
        
        // Reset angle tracking when component is disabled
        previousTargetAngle = float.NaN;
    }

}
