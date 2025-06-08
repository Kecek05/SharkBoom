using System.Collections;
using UnityEngine;

public class RotateTowardsVelocityComponent : BaseItemComponent
{
    [Tooltip("How fast the spear rotates to align with its velocity.")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float velocityThreshold = 4f; // Minimum velocity to consider for rotation
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    
    private Coroutine rotateCoroutine;

    protected override void OnEnableComponent()
    {
        //Rotate the object to the right direction that the player is facing and get the right rotation, if is positive or if is negative
        
        /*if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }*/
    }

    protected override void DoComponentLogic()
    {
        rotateCoroutine = StartCoroutine(RotateObject()); //if not null start the coroutine and assign it to rotateCoroutine
    }

    private IEnumerator RotateObject()
    {
        while(true)
        {
            Vector3 vel = rb.linearVelocity;
            if (vel.sqrMagnitude > velocityThreshold)
            {
                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.fixedDeltaTime);

                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            yield return waitForFixedUpdate;
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
    }

}
