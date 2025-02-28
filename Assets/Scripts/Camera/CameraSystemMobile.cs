using Sortify;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static DragAndShoot;

public class CameraSystemMobile : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private InputReader inputReader;
    //[SerializeField] private DragAndShoot dragAndShoot;

    [BetterHeader("Settings")]
    [SerializeField] private float dragSpeed = 1f;
    private bool dragPanMoveActive = false;
    private Vector2 lastTouchPosition;
    private Vector3 moveDirection;


    private void Start()
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;

        DragAndShoot dragInstance = FindObjectOfType<DragAndShoot>();
        dragInstance.OnDragging += DragAndShoot_OnDragging;
        Debug.Log(dragInstance);
    }

    private void OnDestroy()
    {
        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        if (context.started) // When we press the screen
        {
            dragPanMoveActive = true;
            MoveStarted();
        }
        else if (context.canceled) // When we release the screen
        {
            dragPanMoveActive = false;
            MoveFinish();
        }
    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        if (dragPanMoveActive)
        {
            Vector2 currentTouchPosition = context.ReadValue<Vector2>(); // Get the current position of the finger while the finger is on the screen

            if (lastTouchPosition != Vector2.zero)
            {
                Vector2 movementDelta = currentTouchPosition - lastTouchPosition;
                MoveCamera(movementDelta); // move the camera with the diffence between the last touch position and the current touch position
            }

            lastTouchPosition = currentTouchPosition;
        }
    }

    private void MoveStarted()
    {
        lastTouchPosition = Vector2.zero; 
    }

    private void MoveFinish()
    {
        lastTouchPosition = Vector2.zero; 
    }

    private void MoveCamera(Vector2 movementDelta)
    {
        moveDirection = new Vector3(-movementDelta.x, -movementDelta.y, 0) * dragSpeed * Time.deltaTime; 
        transform.position += moveDirection;
    }

    private void DragAndShoot_OnDragging(object sender, OnDraggingEventArgs e)
    {
        Vector3 newPosition = transform.position;
        newPosition.z += e._direction.z; 
        transform.position += newPosition;
    }
}

    
