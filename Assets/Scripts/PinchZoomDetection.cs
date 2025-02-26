using Sortify;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinchZoomDetection : MonoBehaviour
{
    [BetterHeader("References")]

    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraSystem;

    #region Variables

    private Coroutine zoomCoroutine;
    
    private float previousDistance; 
    private float currentDistance;  

    private Vector2 primaryFingerPosition;
    private Vector2 secondaryFingerPosition;

    private float pinchSpeed;

    #endregion

    private void Start()
    {
        inputReader.OnSecondaryTouchContactEvent += InputReader_OnSecondaryTouchContactEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;
        inputReader.OnSecondaryFingerPositionEvent += InputReader_OnSecondaryFingerPositionEvent;
    }

    private void InputReader_OnSecondaryTouchContactEvent(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ZoomStarted();
        }

        if (context.canceled)
        {
            ZoomEnded();
        }
    }

    private void InputReader_OnSecondaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        primaryFingerPosition = context.ReadValue<Vector2>();

    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        secondaryFingerPosition = context.ReadValue<Vector2>();
    }


    private void ZoomStarted()
    {
        if(zoomCoroutine == null)
        {
            zoomCoroutine = StartCoroutine(ZoomDectection());
        }
    }

    private void ZoomEnded()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
    }

    private IEnumerator ZoomDectection()
    {
        while (true)
        {
            currentDistance = Vector2.Distance(primaryFingerPosition, secondaryFingerPosition);
            
            if(currentDistance > previousDistance)
            {
                Vector3 targetPosition = cameraSystem.position;
                targetPosition.z -= 1;
                cameraSystem.position = Vector3.Lerp(cameraSystem.position, targetPosition, Time.deltaTime * pinchSpeed);
            }
            else if (currentDistance < previousDistance)
            {
                Vector3 targetPosition = cameraSystem.position;
                targetPosition.z += 1;
                cameraSystem.position = Vector3.Lerp(cameraSystem.position, targetPosition, Time.deltaTime * pinchSpeed);
            }

            previousDistance = currentDistance;
            yield return null;
        }
    }
}
