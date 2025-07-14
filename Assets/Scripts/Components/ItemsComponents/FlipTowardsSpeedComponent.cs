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

    [BetterHeader("Settings")] 
    [SerializeField] private float movementThreshold = 0.01f;
    
    private Coroutine _flipCoroutine;
    private WaitForSeconds _waitForSpawnTime = new WaitForSeconds(1f);
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    
    protected override void OnEnableComponent()
    {
        StopFlipCoroutine();
    }

    protected override void DoComponentLogic()
    {
        _flipCoroutine ??= StartCoroutine(DoFlipCheck()); //if not null start the coroutine and assign it to spinCoroutine

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
        yield return _waitForFixedUpdate;
        
        bool isFacingRight = true;
        
        Debug.Log($"SWORD - LinearVelocity: {Mathf.Abs(rb.linearVelocity.x)}");  
        if (Mathf.Abs(rb.linearVelocity.x) > movementThreshold)
        {
            bool shouldFaceRight = rb.linearVelocity.x > 0f;

            if (shouldFaceRight != isFacingRight)
            {
                Flip(shouldFaceRight);
                Debug.Log("SWORD - Flip");  
                isFacingRight = shouldFaceRight;
            }
        }
        yield return _waitForSpawnTime;
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
        if (_flipCoroutine != null)
        {
            StopCoroutine(_flipCoroutine);
            _flipCoroutine = null;
        }
    }
}
