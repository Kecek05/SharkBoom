using System;
using System.Collections;
using UnityEngine;

public class BananaItemThrowable : BaseItemThrowableActivable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;

    private Vector3 launchPosition;
    private Coroutine bananaReturnCoroutine; // Start "Boomerang" Courotine

    [Header("Banana (Boomerang) Settings")]
    [SerializeField] private AnimationCurve boomerangCurve;
    [Tooltip("Time to banana return, increase for decrease Banana speed")]
    [SerializeField] private float returnDuration = 1f;
    [SerializeField] private float heightY = 3f;

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

    private IEnumerator ReturnToPlayer(Vector3 startPosition, Vector3 targetPosition)
    {
        float timer = 0f;
        Vector3 endPosition = targetPosition;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float linearTime = Mathf.Clamp01(timer / returnDuration);

            float heightTime = boomerangCurve.Evaluate(linearTime);
            float height = heightTime * heightY; 

            Vector3 curveMovePosition = Vector3.Lerp(startPosition, endPosition, linearTime);
            transform.position = curveMovePosition + Vector3.up * height;

            yield return null;
        }

        transform.position = endPosition;
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
