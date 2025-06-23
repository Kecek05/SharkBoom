using Sortify;
using UnityEngine;

public class FollowTransformWithLocalOffsetComponent : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Transform targetTransform;
    private Vector3 targetPositionWithOffset;
    [Space(5)]

    [BetterHeader("Settings")]
    [SerializeField] private bool isActive = true;
    [Space(1)]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool followScale = false;
    [Space(3)]
    [SerializeField] private Vector3 positionOffset;
    [Space(1)]
    [SerializeField] private Vector3 eulerRotationOffset;
    [Space(3)]

    [BetterHeader("Interpolation Settings")]
    [SerializeField] private bool useInterpolation = false;
    [SerializeField] private float interpolationSpeed = 5f;


    //DEBUG
    public bool IsActive => isActive;
    public Transform TargetTransform => targetTransform;

    private void LateUpdate()
    {
        if(!isActive) return;
        
        if (targetTransform == null) return;
        
        // Apply positionOffset in the target's local space
        targetPositionWithOffset = targetTransform.TransformPoint(positionOffset);

        if (useInterpolation)
        {
            MoveWithInterpolation();
        }
        else
        {
            Move();
        }
    }

    private void MoveWithInterpolation()
    {
        if (followPosition)
            transform.position = Vector3.Lerp(transform.position, targetPositionWithOffset, interpolationSpeed * Time.deltaTime);

        if (followRotation)
        {
            Quaternion targetRotWithOffset = targetTransform.rotation * Quaternion.Euler(eulerRotationOffset);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotWithOffset, interpolationSpeed * Time.deltaTime);
        }

        if (followScale)
            transform.localScale = Vector3.Lerp(transform.localScale, targetTransform.localScale, interpolationSpeed * Time.deltaTime);
    }

    private void Move()
    {
        if (followPosition) transform.position = targetPositionWithOffset;

        if (followRotation)
        {
            transform.rotation = targetTransform.rotation * Quaternion.Euler(eulerRotationOffset);
        }

        if (followScale) transform.localScale = targetTransform.localScale;
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

    public void SetPositionOffset(Vector3 offset)
    {
        positionOffset = offset;
    }

    public void SetUseInterpolation(bool value)
    {
        useInterpolation = value;
    }
}
