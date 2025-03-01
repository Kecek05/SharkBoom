using Sortify;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{

    [SerializeField] private InputReader inputReader; 

    private Coroutine zoomCoroutine;

    private float previousDistance;
    private float currentDistance;

    private Vector2 primaryFingerPosition;
    private Vector2 secondaryFingerPosition;

    private Vector3 cameraSystemPosition;

    [BetterHeader("Variables")]

    [SerializeField] private float pinchSpeed = 100f;
    [SerializeField] private float pinchDragSpeed = 10f;
    [SerializeField] private float maxZoomInClamp = -2f;
    [SerializeField] private float maxZoomOutClamp = 10f;
    [SerializeField] private CinemachineCamera cinemachineCamera;

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
            ZoomStarted(); // when we have two fingers on the screen
            CameraManager.Instance.SetCameraState(CameraManager.CameraState.Zoom);
        }

        if (context.canceled)
        {
            ZoomEnded();
            CameraManager.Instance.SetCameraState(CameraManager.CameraState.Default);
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
        if (zoomCoroutine == null)
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

            if (currentDistance > previousDistance) // zoom in
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
        cameraSystemPosition = CameraManager.Instance.CameraObjectToFollow.position;
        cameraSystemPosition.z += value;
        
        cameraSystemPosition.z = Mathf.Clamp(cameraSystemPosition.z, -10f, -2f);
        CameraManager.Instance.CameraObjectToFollow.position = Vector3.Lerp(CameraManager.Instance.CameraObjectToFollow.position, cameraSystemPosition, Time.deltaTime * pinchSpeed);
    }

    public float CalculateZoomByForce(float force)
    {

        return force;
    }

    private void OnDestroy()
    {
        inputReader.OnSecondaryTouchContactEvent -= InputReader_OnSecondaryTouchContactEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
        inputReader.OnSecondaryFingerPositionEvent -= InputReader_OnSecondaryFingerPositionEvent;
    }
}
