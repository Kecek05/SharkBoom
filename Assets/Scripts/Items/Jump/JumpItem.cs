using System.Collections;
using UnityEngine;

public class JumpItem : BaseItemThrowable, IActivable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;

    public void Activate() //DEBUG
    {
        Debug.Log("Jump Item Activated");
    }

    protected override void ItemReleased(float force, Vector3 direction)
    {
        base.ItemReleased(force, direction);
        StartCoroutine(PlayerFollowFirework());
    }

    private IEnumerator PlayerFollowFirework()
    {
        currentFollowingTime = 0f;
        while (currentFollowingTime < followingTime)
        {
            shooterTransform.position = transform.position;
            currentFollowingTime += Time.deltaTime;
            yield return null;
        }
    }
}
