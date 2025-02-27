using Sortify;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinchZoomDetection : MonoBehaviour
{
    #region References

    [BetterHeader("References")]

    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraSystem; // object that camera is following

    #endregion

    #region Variables

    private Coroutine zoomCoroutine;
    
    private float previousDistance; 
    private float currentDistance;  

    private Vector2 primaryFingerPosition;
    private Vector2 secondaryFingerPosition;

    [BetterHeader("Variables")]

    [SerializeField] private float pinchSpeed = 100f;

    #endregion

    private void Start()
    {
        inputReader.OnSecondaryTouchContactEvent += InputReader_OnSecondaryTouchContactEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;
        inputReader.OnSecondaryFingerPositionEvent += InputReader_OnSecondaryFingerPositionEvent;
    }

    private void OnDestroy()
    {
        inputReader.OnSecondaryTouchContactEvent -= InputReader_OnSecondaryTouchContactEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
        inputReader.OnSecondaryFingerPositionEvent -= InputReader_OnSecondaryFingerPositionEvent;
    }

    private void InputReader_OnSecondaryTouchContactEvent(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ZoomStarted(); // when we have two fingers on the screen
        }

        if (context.canceled)
        {
            ZoomEnded(); 
        }
    }

    private void InputReader_OnSecondaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        primaryFingerPosition = context.ReadValue<Vector2>(); // just grab the position of the first finger

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
            
            if(currentDistance > previousDistance) // zoom in
            {
                Vector3 targetPosition = cameraSystem.position;
                targetPosition.z -= 1; 
                cameraSystem.position = Vector3.Lerp(cameraSystem.position, targetPosition, Time.deltaTime * pinchSpeed); 
            }
            else if (currentDistance < previousDistance) // zoom out
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
