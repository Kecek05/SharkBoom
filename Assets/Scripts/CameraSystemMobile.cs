using Unity.Cinemachine;
using UnityEngine;

public class CameraSystemMobile : MonoBehaviour
{
    #region Variables 

    [SerializeField] private float dragSpeed = 1f;
    // [SerializeField] private float zoomSpeed = 0.5f;
    private bool dragPanMoveActive = false;
    private Vector2 lastTouchPosition;

    #endregion



    void Update()
    {
        HandleInput();
        MoveCamera();
    }

    private void HandleInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragPanMoveActive = true;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                dragPanMoveActive = false;
            }
        }

    }

    private void MoveCamera()
    {
        if (dragPanMoveActive)
        {
            Vector2 movementDelta = Vector2.zero;

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                movementDelta = touch.position - lastTouchPosition;
                lastTouchPosition = touch.position;
            }

            Vector3 moveDir = new Vector3(-movementDelta.x, -movementDelta.y, 0) * dragSpeed * Time.deltaTime;
            transform.position += moveDir;

        }
    }


}