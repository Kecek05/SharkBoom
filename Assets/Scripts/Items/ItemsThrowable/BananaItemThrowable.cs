using System;
using System.Collections;
using UnityEngine;

public class BananaItemThrowable : BaseItemThrowableActivable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private GameObject bananaMesh;

    private Vector3 launchPosition;
    private Coroutine bananaReturnCoroutine; // Start "Boomerang" Courotine

    [Header("Banana (Boomerang) Settings")]
    [SerializeField] private AnimationCurve boomerangCurve;
    [Tooltip("Time to banana return, increase for decrease Banana speed")]
    [SerializeField] private float returnDuration = 1f;
    [Tooltip("Banana curve height, negative values make it curve for down")]
    [SerializeField] private float heightY = 3f;

    private WaitForSecondsRealtime waitToDestroy = new WaitForSecondsRealtime(1.5f);

    protected void OnEnable()
    {
        base.OnEnable();
        bananaMesh.SetActive(true); // Ensure the banana mesh is active when the item is enabled
        rb.isKinematic = false;
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);
        spinObjectComponent.EnableComponent();
        spinObjectComponent.StartComponentLogic();

        launchPosition = transform.position;
    }

    protected override void ActivateItem()
    {
        Vector3 startReturnPosition = transform.position;

        //if (bananaReturnCoroutine != null)
        //{
        //    StopCoroutine(bananaReturnCoroutine);
        //}

        bananaReturnCoroutine = StartCoroutine(ReturnToPlayer(startReturnPosition, launchPosition));
    }

    private IEnumerator ReturnToPlayer(Vector3 startPosition, Vector3 targetPosition)
    {
        float timer = 0f;
        Vector3 endPosition = targetPosition;

        spinObjectComponent.StartComponentLogic();
        
        while (timer < returnDuration)
        {

            timer += Time.deltaTime;
            float linearTime = Mathf.Clamp01(timer / returnDuration);

            float heightTime = boomerangCurve.Evaluate(linearTime);
            float height = heightTime * heightY; 

            Vector3 curveMovePosition = Vector3.Lerp(startPosition, endPosition, linearTime);
            curveMovePosition.z = startPosition.z; 
            transform.position = curveMovePosition + Vector3.up * height;

            yield return null;
        }
        transform.position = endPosition;
        bananaReturnCoroutine = null;

        bananaMesh.SetActive(false);
        rb.isKinematic = true;
        yield return waitToDestroy;

        DestroyItem();
    }

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        if(!itemActivated)
            spinObjectComponent.DisableComponent();
        
        //Dont allow to activate the item if collided
        itemCanBeActivated = false;
    }
    
    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
        if (bananaReturnCoroutine != null)
        {
            StopCoroutine(bananaReturnCoroutine);
            bananaReturnCoroutine = null;
        }
        bananaMesh.SetActive(true); // Ensure the banana mesh is active when the item is enabled
        rb.isKinematic = false;
    }
}
