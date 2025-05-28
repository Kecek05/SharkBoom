using System;
using System.Collections;
using UnityEngine;

public class BananaItemThrowable : BaseItemThrowableActivable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private BaseCollisionController collisionController;

    private Vector3 launchPosition;
    private Coroutine bananaReturnCoroutine; // Start "Boomerang" Courotine

    [Header("Banana (Boomerang) Settings")]
    [SerializeField] private AnimationCurve boomerangCurve;
    [Tooltip("Time to banana return, increase for decrease Banana speed")]
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
    }

    protected override void ActivateItem()
    {
        itemActivated = true;
        Vector3 startReturnPosition = transform.position;

        if (bananaReturnCoroutine != null)
        {
            StopCoroutine(bananaReturnCoroutine);
        }

        bananaReturnCoroutine = StartCoroutine(ReturnToPlayer(startReturnPosition, launchPosition));
    }

    private IEnumerator ReturnToPlayer(Vector3 returnPosition, Vector3 targetPosition)
    {
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        float timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float timeOfReturn = Mathf.Clamp01(timer / returnDuration); // we use Clamp01 for not extrapolate the return pos

           
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
