using UnityEngine;

public class DragAndShoot : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;

    private float forceMultiplier;
    private float maxForceMultiplier = 100;
    private bool isDragging = false;

    [SerializeField] private Transform spawnPos;
    [SerializeField] private Transform shperePos;
    
    [SerializeField] private Rigidbody rb;


    private void Update()
    {
        StartDrag();
        MouseDrag();
    }

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray rayStart = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(rayStart, out hit) && hit.collider.gameObject == gameObject)
            {
                startPos = rayStart.origin;
                isDragging = true;
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            shperePos.position = spawnPos.position;
        }
    }

    private void MouseDrag()
    {
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Ray rayEnd = Camera.main.ScreenPointToRay(Input.mousePosition);
            endPos = rayEnd.origin;
            ReleaseDrag();
        }
    }

    private void ReleaseDrag()
    {
        Vector3 direction = (startPos - endPos).normalized;
        forceMultiplier = Vector3.Distance(startPos, endPos);

        if (forceMultiplier > 0.15) 
        {
            forceMultiplier = 0.15f;
        }

        rb.AddForce(direction * forceMultiplier * maxForceMultiplier, ForceMode.Impulse);
        isDragging = false;
    }
}
