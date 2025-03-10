using System.Collections;
using UnityEngine;

public class JumpItem : BaseItemThrowable, IActivable, IFollowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;

    public void Activate() //DEBUG
    {
        Debug.Log("Jump Item Activated");
    }

    public void Follow(Transform objectToFollow)
    {
        objectToFollowTransform = objectToFollow;
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
            objectToFollowTransform.position = transform.position;
            currentFollowingTime += Time.deltaTime;
            yield return null;
        }
    }
}
