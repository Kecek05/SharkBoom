using DG.Tweening;
using Sortify;
using UnityEngine;

public class FollowTransformWithTreshHoldComponent : MonoBehaviour
{
 [Header("References")]
    [SerializeField] private Transform targetTransform;

    [Header("Settings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;
    [SerializeField] private Vector3 positionOffset;
    [Tooltip("Min movement (units) to trigger follow")]
    [SerializeField] private float positionThreshold = 0.1f;
    [Tooltip("Min rotation change (degrees) to trigger follow")]
    [SerializeField] private float rotationThreshold = 5f;
    [Tooltip("Tween duration (secs) when movement is triggered")]
    [SerializeField] private float moveDuration = 0.5f;
    [Tooltip("Tween ease type")]
    [SerializeField] private Ease tweenEase = Ease.OutCubic;

    private Vector3 lastTargetPos;
    private Quaternion lastTargetRot;
    private Tween positionTween, rotationTween;

    private void Start()
    {
        lastTargetPos = transform.position;
        lastTargetRot = transform.rotation;
    }

    private void LateUpdate()
    {
        if (!isActive || targetTransform == null) return;

        var desiredPos = targetTransform.position + targetTransform.TransformDirection(positionOffset);
        var desiredRot = targetTransform.rotation;

        // Position tweens
        if (followPosition)
        {
            if (Vector3.Distance(lastTargetPos, desiredPos) >= positionThreshold)
            {
                positionTween?.Kill();
                positionTween =
                    transform.DOMove(desiredPos, moveDuration)
                             .SetEase(tweenEase);
                lastTargetPos = desiredPos;
            }
        }

        // Rotation tweens
        if (followRotation)
        {
            if (Quaternion.Angle(lastTargetRot, desiredRot) >= rotationThreshold)
            {
                rotationTween?.Kill();
                rotationTween =
                    transform.DORotateQuaternion(desiredRot, moveDuration)
                             .SetEase(tweenEase);
                lastTargetRot = desiredRot;
            }
        }
    }
}
