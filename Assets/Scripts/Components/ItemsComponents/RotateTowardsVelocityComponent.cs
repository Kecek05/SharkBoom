using System.Collections;
using UnityEngine;

public class RotateTowardsVelocityComponent : BaseItemComponent
{
    [Tooltip("How fast the spear rotates to align with its velocity.")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody2D rb;

    private Coroutine rotateCoroutine;

    protected override void OnEnableComponent()
    {
        //Rotate the object to the right direction that the player is facing and get the right rotation, if is positive or if is negative
    }

    protected override void DoComponentLogic()
    {
        rotateCoroutine ??= StartCoroutine(RotateObject()); //if not null start the coroutine and assign it to rotateCoroutine
    }

    private IEnumerator RotateObject()
    {
        while(true)
        {
            Vector2 vel2D = rb.linearVelocity;
            if (vel2D.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(vel2D.y, vel2D.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            yield return new WaitForFixedUpdate();
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
