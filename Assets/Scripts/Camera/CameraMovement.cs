using Sortify;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private InputReader inputReader;

    [BetterHeader("Settings")]
    [SerializeField] private float dragSpeed = 1f;
    private bool dragPanMoveActive = false;
    private Vector2 lastTouchPosition;


    private void Start()
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;
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
        if (dragPanMoveActive && this.enabled == true)
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
        Vector3 moveDir = new Vector3(-movementDelta.x, -movementDelta.y, 0) * dragSpeed * Time.deltaTime; 
        CameraManager.Instance.CameraObjectToFollow.position += moveDir;
    }
}

    
