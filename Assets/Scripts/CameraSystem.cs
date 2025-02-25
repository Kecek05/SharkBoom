using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    #region Variables

    [SerializeField] private float dragSpeed = 1f;
    private bool dragPanMoveActive = false; // bool for checking if the drag move is active
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
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition; // we grab the actual mouse position and subtract the last mouse position 
            Vector3 moveDir = new Vector3(-mouseMovementDelta.x, -mouseMovementDelta.y, 0) * dragSpeed * Time.deltaTime; // here we calculate the moveDirection, we use the - because we want to move the camera in the opposite direction of the mouse movement
            transform.position += moveDir; // We apply the moveDir for a transform that Camera is following, so the camera moves
            lastMousePosition = Input.mousePosition; // we update the lastMousePosition to the current mouse position
        }
    }
}
