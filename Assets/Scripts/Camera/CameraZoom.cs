using Sortify;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    #region References

    [BetterHeader("References")]
    [SerializeField] private Transform cameraObjectToFollow; // object that camera is following

    #endregion

    #region Variables

    private Coroutine zoomCoroutine;
    
    private float previousDistance; 
    private float currentDistance;  

    private Vector2 primaryFingerPosition;
    private Vector2 secondaryFingerPosition;

    private Vector3 cameraSystemPosition;

    [BetterHeader("Variables")]

    [SerializeField] private float pinchSpeed = 100f;

    #endregion

    private void Start()
    {
        CameraManager.Instance.InputReader.OnSecondaryTouchContactEvent += InputReader_OnSecondaryTouchContactEvent;
        CameraManager.Instance.InputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;
        CameraManager.Instance.InputReader.OnSecondaryFingerPositionEvent += InputReader_OnSecondaryFingerPositionEvent;
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
                ChangeZoom(1f);
            }
            else if (currentDistance < previousDistance) // zoom out
            {
                ChangeZoom(-1f);
            }

            previousDistance = currentDistance;
            yield return null;
        }
    }


    public void ChangeZoom(float value)
    {
        cameraSystemPosition = cameraObjectToFollow.position;
        cameraSystemPosition.z += value;

        cameraSystemPosition.z = Mathf.Clamp(cameraSystemPosition.z, -10f, -2f);
        cameraObjectToFollow.position = Vector3.Lerp(cameraObjectToFollow.position, cameraSystemPosition, Time.deltaTime * pinchSpeed);
    }

    private void OnDestroy()
    {
        CameraManager.Instance.InputReader.OnSecondaryTouchContactEvent -= InputReader_OnSecondaryTouchContactEvent;
        CameraManager.Instance.InputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
        CameraManager.Instance.InputReader.OnSecondaryFingerPositionEvent -= InputReader_OnSecondaryFingerPositionEvent;
    }
}
