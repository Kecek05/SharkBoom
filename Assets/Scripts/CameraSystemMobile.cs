using Sortify;
using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSystemMobile : MonoBehaviour
{
    #region References

    [BetterHeader("References")]

    [SerializeField] private InputReader inputReader;
    [SerializeField] private PinchZoomDetection pinchZoomDetection;

    #endregion

    #region Variables 

    [BetterHeader("Variables")]

    [SerializeField] private float dragSpeed = 1f;
    // [SerializeField] private float zoomSpeed = 0.5f;
    private bool dragPanMoveActive = false;
    private Vector2 lastTouchPosition;

    #endregion


    private void Start()
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
    }

    private void OnDisable()
    {
        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
    }

    private void InputReader_OnTouchPressEvent(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {

        if(Input.touchCount != 1)
        {
            return;
        }

        if (context.started)
        {
            dragPanMoveActive = true;
            lastTouchPosition = context.ReadValue<Vector2>();
            MoveCamera();
        }
        else if (context.canceled)
        {
            dragPanMoveActive = false;
        }
    }

    private void MoveCamera()
    {
        if (!dragPanMoveActive || Input.touchCount != 1 || pinchZoomDetection.zoomCourotine != null)
        {
            Vector2 movementDelta = Vector2.zero;
            Touch touch = Input.GetTouch(0);
            movementDelta = touch.position - lastTouchPosition;
            lastTouchPosition = touch.position;
            Vector3 moveDir = new Vector3(-movementDelta.x, -movementDelta.y, 0) * dragSpeed * Time.deltaTime;
            transform.position += moveDir;
        }
    }
 
}