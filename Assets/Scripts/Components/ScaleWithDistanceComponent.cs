using DG.Tweening;
using Sortify;
using UnityEngine;

public class ScaleWithDistanceComponent : MonoBehaviour
{
    [BetterHeader("Settings")]
    [Tooltip("Distance from camera where the object should have the default scale.")]
    [SerializeField] private  float referenceDistance = 10f;
    [Tooltip("The default scale when the camera is at the referenceDistance.")]
    [SerializeField] private float referenceScale = 1f;
    [Space(4)]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 2f;
    [Space(4)]
    
    [Header("Tween Settings")]
    [Tooltip("How long (in seconds) the scale tween should take when the target changes.")]
    [SerializeField] private float tweenDuration = 0.2f;
    [Tooltip("Ease type for the scaling tween.")]
    [SerializeField] private Ease tweenEase = Ease.OutQuad;
    
    private Camera targetCamera;
    private Tween  scaleTween;
    private Transform cachedTransform;
    private float  lastTargetScale = -1f;

    public bool debugTurnOffDoTween = false;
    
    private void Awake()
    {
        cachedTransform = transform;
        targetCamera    = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        float distance = Vector3.Distance(targetCamera.transform.position, transform.position);
        
        float scale = (distance / referenceDistance) * referenceScale;

        float clampedScale = Mathf.Clamp(scale, minScale, maxScale);

        if (debugTurnOffDoTween)
        {
            transform.localScale = new Vector3(clampedScale, clampedScale, clampedScale);
            return;
        }
        
        if (Mathf.Approximately(clampedScale, lastTargetScale)) //Too close to change
            return;
        
        scaleTween?.Kill();
        
        scaleTween = cachedTransform
            .DOScale(Vector3.one * clampedScale, tweenDuration)
            .SetEase(tweenEase);

        lastTargetScale = clampedScale;
        
        //transform.localScale = new Vector3(clampedScale, clampedScale, clampedScale);
        
    }
}
