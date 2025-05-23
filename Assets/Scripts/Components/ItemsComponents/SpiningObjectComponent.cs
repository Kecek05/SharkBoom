using System.Collections;
using UnityEngine;

public class SpiningObjectComponent : BaseItemComponent
{
    [SerializeField] private float spiningSpeed = 300f;
    private Coroutine spinCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    protected override void OnEnableComponent()
    {
        //Rotate the object to the right direction that the player is facing
    }

    protected override void DoComponentLogic()
    {
        spinCoroutine ??= StartCoroutine(SpinObject()); //if not null start the coroutine and assign it to spinCoroutine
    }

    private IEnumerator SpinObject()
    {
        while (true)
        {
            // Rotate the object around its Z-axis
            transform.Rotate(0, 0, -spiningSpeed * Time.deltaTime);

            yield return waitForFixedUpdate;
        }
        spinCoroutine = null;
    }

    protected override void OnDisableComponent()
    {
        if(spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }
    }
}
