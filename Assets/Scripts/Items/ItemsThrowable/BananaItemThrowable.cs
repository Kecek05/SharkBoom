using System;
using System.Collections;
using UnityEngine;

public class BananaItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private BaseCollisionController collisionController;

    private Vector3 launchPosition;
    private Coroutine bananaReturnCoroutine; // Start "Boomerang" Courotine

    [Header("Banana (Boomerang) Settings")]
    [Tooltip("Time before the banana starts returning")]
    [SerializeField] private float flightDuration = 1f;
    [Tooltip("Time to banana return")]
    [SerializeField] private float returnDuration = 1f;

    public override void Initialize(Transform parent)
    {
        base.Initialize(parent);
        collisionController.OnCollided += OnCollided;
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);
        spinObjectComponent.EnableComponent();
        spinObjectComponent.StartComponentLogic();

        launchPosition = transform.position;

        if (bananaReturnCoroutine != null)
        {
            StopCoroutine(bananaReturnCoroutine);
        }

        bananaReturnCoroutine = StartCoroutine(ReturnToPlayer());
    }

    private IEnumerator ReturnToPlayer()
    {
        yield return new WaitForSeconds(flightDuration);

        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        float timer = 0f;
        Vector3 returnInitialPos = transform.position;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float timeOfReturn = Mathf.Clamp01(timer / returnDuration); // we use Clamp01 for not extrapolate the return pos
            transform.position = Vector3.Slerp(returnInitialPos, launchPosition, timeOfReturn);
            yield return null;
        }
    }

    private void OnCollided(GameObject collidedObj)
    {
        spinObjectComponent.DisableComponent();
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        collisionController.OnCollided -= OnCollided; //Subscribe to the collision event
        spinObjectComponent.DisableComponent();

        if (bananaReturnCoroutine != null)
        {
            StopCoroutine(bananaReturnCoroutine);
            bananaReturnCoroutine = null;
        }
    }
}
