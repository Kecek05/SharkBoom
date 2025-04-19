using UnityEngine;

public class LookAtTransformComponent : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;

    private void LateUpdate()
    {
        if (targetTransform != null)
        {
            transform.LookAt(targetTransform.position);
        }
    }

}
