using Sortify;
using UnityEngine;

public class FollowTransformComponent : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Transform targetTransform;
    [Space(5)]

    [BetterHeader("Settings")]
    [SerializeField] private bool isActive = true;
    [Space(1)]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool followScale = false;
    [Space(3)]
    [SerializeField] private Vector3 positionOffset;

    //DEBUG
    public bool IsActive => isActive;
    public Transform TargetTransform => targetTransform;

    private void LateUpdate()
    {
        if(!isActive) return;

        if (targetTransform != null)
        {
            if (followPosition) transform.position = targetTransform.position + positionOffset;

            if (followRotation) transform.rotation = targetTransform.rotation;

            if (followScale) transform.localScale = targetTransform.localScale;
        }
    }

    public void SetTarget(Transform target)
    {
        if(target == null) return;
        targetTransform = target;
    }

    public void EnableComponent()
    {
        isActive = true;
    }

    public void DisableComponent()
    {
        isActive = false;
    }

    public void SetFollowPosition(bool value)
    {
        followPosition = value;
    }

    public void SetFollowRotation(bool value)
    {
        followRotation = value;
    }

    public void SetFollowScale(bool value)
    {
        followScale = value;
    }
}
