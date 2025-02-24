using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    private bool dragPanMoveActive = false; 
    private Vector3 startMousePos;
    private Vector3 lastMousePos; 
     [SerializeField] private float dragSpeed = 1f;

    void Update()
    {
        HandleInput();
        MoveCamera();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragPanMoveActive = true;
            Ray rayStart = Camera.main.ScreenPointToRay(Input.mousePosition);
            startMousePos = rayStart.origin; 
            lastMousePos = startMousePos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragPanMoveActive = false;
        }
    }

    private void MoveCamera()
    {
        if (dragPanMoveActive)
        {
            Ray rayCurrent = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 currentMousePos = rayCurrent.origin;

            Vector3 mouseMovementDelta = currentMousePos - lastMousePos;
            Vector3 moveDir = new Vector3(-mouseMovementDelta.x, -mouseMovementDelta.y, 0);
            transform.position += moveDir * dragSpeed;
            lastMousePos = currentMousePos;
        }
    }
}
