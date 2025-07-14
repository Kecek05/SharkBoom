using System;
using System.Collections;
using Sortify;
using UnityEngine;

public class FlipTowardsSpeedComponent : BaseItemComponent
{
    [BetterHeader("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform objectToFlip;
    [Space(5)]
    [SerializeField] private bool inverted = false;
    
    private Coroutine flipCoroutine;
    private bool isFlipping = false;
    protected override void OnEnableComponent()
    {
        StopFlipCoroutine();
    }

    protected override void DoComponentLogic()
    {
        flipCoroutine ??= StartCoroutine(DoFlipCheck()); //if not null start the coroutine and assign it to spinCoroutine

    }

    protected override void OnDisableComponent()
    {
        StopFlipCoroutine();
        Vector3 euler = transform.eulerAngles;
        euler.y = 0f;
        transform.eulerAngles = euler;
    }

    private IEnumerator DoFlipCheck()
    {
        bool isFacingRight = true;
        while (true)
        {

            // if (Mathf.Abs(rb.linearVelocity.x) < 0.01f) continue; // ignore small movements

            bool shouldFaceRight = rb.linearVelocity.x > 0f;

            if (shouldFaceRight != isFacingRight)
            {
                Flip(shouldFaceRight);
                Debug.Log($"IsRight: {isFacingRight}, Velocity: {rb.linearVelocity.x}");
                isFacingRight = shouldFaceRight;
            }

            yield return new WaitForFixedUpdate();
        }
    }
    
    private bool faceRightAtZeroRotation = true; 

    private void Flip(bool faceRight)
    {
        objectToFlip.localScale = new Vector3(faceRight ? 1f : -1f, objectToFlip.localScale.y, objectToFlip.localScale.z);
        
        // float yRotation = faceRightAtZeroRotation 
        //     ? (faceRight ? 0f : 180f)
        //     : (faceRight ? 180f : 0f);
        //
        // Vector3 euler = transform.eulerAngles;
        // euler.y = yRotation;
        // transform.eulerAngles = euler;
    }
    
    public void StopFlipCoroutine()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }
        isFlipping = false;
    }
}
