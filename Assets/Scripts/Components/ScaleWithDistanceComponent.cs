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
    
    private Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main; 
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        float distance = Vector3.Distance(targetCamera.transform.position, transform.position);
        
        float scale = (distance / referenceDistance) * referenceScale;

        float clampedScale = Mathf.Clamp(scale, minScale, maxScale);

        transform.localScale = new Vector3(clampedScale, clampedScale, clampedScale);
        
    }
}
