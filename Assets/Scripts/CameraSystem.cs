using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    #region Variables

    [SerializeField] private float dragSpeed = 1f;
    private bool dragPanMoveActive = false;
    private Vector2 lastMousePosition;

    #endregion


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
            lastMousePosition = Input.mousePosition; // input.mousePosition holds the current mouse position
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
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;
            Vector3 moveDir = new Vector3(-mouseMovementDelta.x, -mouseMovementDelta.y, 0) * dragSpeed * Time.deltaTime;
            transform.position += moveDir;
            lastMousePosition = Input.mousePosition;
        }
    }
}
