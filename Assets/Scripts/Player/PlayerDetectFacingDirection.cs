using Sortify;
using System;
using System.Collections;
using UnityEngine;

public class PlayerDetectFacingDirection : DragListener
{
    /// <summary>
    /// Called when the look orientation is changed. Pass if is looking right
    /// </summary>
    public event Action<bool> OnRotationChanged;


    [BetterHeader("References")]
    [SerializeField] private Transform playerGfxTransform;
    [SerializeField] private PlayerDragController playerDragController;
    [Tooltip("Value to be add to not rotate the object to close to the 90 degrees")]
    [SerializeField] private float angleOffset = 0.5f;

    //Based on the direction of the other player, the default direction is right is true when the enemy is on the right side
    private bool isDefaultDirectionRight = false;

    private bool isDirectionRight = false;

    protected override void DoOnSpawn()
    {
        //reset to default direction
        isDirectionRight = isDefaultDirectionRight;
    }

    protected override void DoOnDragChange(float forcePercent, float andlePercent)
    {

        if (playerDragController.GetOpositeFingerPos().x > playerGfxTransform.position.x + angleOffset)
        {
            //right
            if(isDirectionRight) return; //do nothing if the direction is already right

            OnRotationChanged?.Invoke(true);

            isDirectionRight = true;

        }
        else if (playerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x - angleOffset)
        {
            //left
            if(!isDirectionRight) return; //do nothing if the direction is already left

            isDirectionRight = false;

            OnRotationChanged?.Invoke(false);
        }

        Debug.Log($"Right: {isDirectionRight} ");
    }

    protected override void DoOnDragRelease()
    {
        StartCoroutine(DelayToChangeRotationToDefault());
    }

    private IEnumerator DelayToChangeRotationToDefault()
    {

        yield return null;
    }

}
