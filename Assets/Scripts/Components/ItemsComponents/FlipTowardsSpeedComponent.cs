using System.Collections;
using Sortify;
using UnityEngine;

public class FlipTowardsSpeedComponent : BaseItemComponent
{
    [BetterHeader("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform objectToFlip;
    [Space(5)]
    [SerializeField] private bool inverted;

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
        objectToFlip.localScale = Vector3.one;
    }

    private IEnumerator DoFlipCheck()
    {
        yield return _waitForFixedUpdate; // Ensure that physics will be updated before checking the velocity
        
        Debug.Log($"SWORD - {transform.eulerAngles}");
        
        if (Mathf.Abs(rb.linearVelocity.x) > movementThreshold)
        {
            bool shouldFaceRight = rb.linearVelocity.x > 0f;
            Flip(shouldFaceRight);
            Debug.Log($"SWORD - LinearVelocity: {rb.linearVelocity.x}, ShouldFaceRight: {shouldFaceRight}");  
        }
        yield return _waitForSpawnTime;
    }
    
   // private bool faceRightAtZeroRotation = true; 

    private void Flip(bool faceRight)
    {
        Vector3 scale = objectToFlip.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        objectToFlip.localScale = scale;
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
