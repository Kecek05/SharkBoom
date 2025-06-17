using UnityEngine;

/// <summary>
/// Scales a World-Space Canvas so it appears a fixed size on screen regardless of distance.
/// At `referenceDistance` units from the camera, the Canvas has scale = referenceScale; 
/// at other distances its scale is adjusted proportionally.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasScaleWithDistanceComponent : MonoBehaviour
{
    [Tooltip("Distance from camera where the Canvas should have the base scale.")]
    public float referenceDistance = 10f;
    [Tooltip("The Canvas scale when at the reference distance (default 1 means original size).")]
    public float referenceScale = 1f;
    [Tooltip("Camera used for distance calculations. If not set, Camera.main is used.")]
    public Camera targetCamera;

    private Transform mTransform;

    void Awake()
    {
        mTransform = transform;
        if (targetCamera == null)
            targetCamera = Camera.main; 
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 1. Compute the current distance from the camera
        float distance = Vector3.Distance(targetCamera.transform.position, mTransform.position);

        // 2. Compute the scale factor so the Canvas appears the same size:
        //    scale = (current distance / reference distance) * referenceScale
        float scale = (distance / referenceDistance) * referenceScale;

        // 3. Apply uniform scaling to the Canvas
        mTransform.localScale = new Vector3(scale, scale, scale);

        // 4. (Optional) Rotate Canvas to face the camera (billboarding)
        mTransform.forward = mTransform.position - targetCamera.transform.position;
    }
}
