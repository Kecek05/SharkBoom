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

    private void Update()
    {
        if (targetTransform != null)
        {
            if (followPosition) transform.position = targetTransform.position;

            if (followRotation) transform.rotation = targetTransform.rotation;

            if (followScale) transform.localScale = targetTransform.localScale;
        }
    }
}
