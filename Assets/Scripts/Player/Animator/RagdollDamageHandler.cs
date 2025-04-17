using UnityEngine;

public class RagdollDamageHandler : MonoBehaviour
{
    [SerializeField] private float maximiumForce;
    [SerializeField] private float maximiumForceTime;
     private float timeMouseButtonDown;
    [SerializeField] private Camera camera;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            timeMouseButtonDown = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                PlayerRagdollEnabler playerRagdollEnabler = hitInfo.collider.GetComponentInParent<PlayerRagdollEnabler>();

                if (playerRagdollEnabler != null)
                {
                    float mouseButtonDownDuration = Time.time - timeMouseButtonDown;
                    float forcePercentage = mouseButtonDownDuration / maximiumForceTime;
                    float forceMagnitude = Mathf.Lerp(1, maximiumForce, forcePercentage);

                    Vector3 forceDirection = playerRagdollEnabler.transform.position - camera.transform.position;
                    forceDirection.y = 1;
                    forceDirection.Normalize();

                    Vector3 force = forceMagnitude * forceDirection;
                    playerRagdollEnabler.TriggerRagdoll(force, hitInfo.point);
                }
            }
        }
    }
}
