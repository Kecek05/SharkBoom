using Sortify;
using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private InputReader inputReader;

    [BetterHeader("Settings")]

    [Tooltip("Velocity of camera movement on drag")] [Range(0, 50)]
    [SerializeField] private float dragMoveSpeed = 1f;

    [Tooltip("Min clamp of x movement ")] [Range(-30, 30)]
    [SerializeField] private int minMovX = -15;

    [Tooltip("Max clamp of x movement ")] [Range(-30, 30)]
    [SerializeField] private int maxMovX = 15;

    [Tooltip("Max clamp of Y movement ")] [Range(-30, 30)]
    [SerializeField] private int minMovY = -10;

    [Tooltip("Min clamp of x movement ")][Range(-30, 30)]
    [SerializeField] private int maxMovY = 10;


    private bool dragMoveActive = false; // hold if the drag move is active
    private Vector2 lastTouchPosition;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        if (context.started) // When we press the screen
        {
            dragMoveActive = true;
            MoveStarted();
            Debug.Log("Started dragging");
        }
        else if (context.canceled) // When we release the screen
        {
            dragMoveActive = false;
            MoveFinish();
            Debug.Log("Finished dragging");
        }
    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        if (dragMoveActive && this.enabled == true) // this.enabled is to check if the camera state is on move
        {
            Vector2 currentTouchPosition = context.ReadValue<Vector2>(); // Get the current position of the finger while the finger is on the screen

            if (lastTouchPosition != Vector2.zero) // if the touch is moving
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
        Vector3 moveDir = new Vector3(-movementDelta.x, -movementDelta.y, 0) * dragMoveSpeed * Time.deltaTime; // we put a negative value to invert the movement, making the sensation of dragging the camera
        cameraManager.CameraObjectToFollow.position = new Vector3(
            Mathf.Clamp(cameraManager.CameraObjectToFollow.position.x + moveDir.x, minMovX, maxMovX), 
            Mathf.Clamp(cameraManager.CameraObjectToFollow.position.y + moveDir.y, minMovY, maxMovY),  
            cameraManager.CameraObjectToFollow.position.z 
        );  // Basically we get the pos of camera and add the movement direction of the camera, and clamp the values to the min and max values
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;
        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
    }
}

    
