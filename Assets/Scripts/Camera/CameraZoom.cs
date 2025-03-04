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

    private Vector3 cameraObjectFollowPos;

    [BetterHeader("Variables")]

    [Tooltip("Think like a scope of a sniper, min = more distant of player")]
    [SerializeField] private float minZoom = -15f;
    [Tooltip("Think like a scope of a sniper, max = more close of player")]
    [SerializeField] private float maxZoom = 1f;
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

    private void ZoomEnded()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
    }
    

    /// <summary>
    /// Function for update the zoom of the camera
    /// </summary>
    /// <param name="value">Amount to zoom in/out (negative = zoom out, positive = zoom in)</param>
    /// <param name="zoomSpeed">Speed of the zoom transition</param>
    public void ChangeZoom(float value, float zoomSpeed = 100f)
    {
        cameraObjectFollowPos = CameraManager.Instance.CameraObjectToFollow.position; // get the current position of the camera
        cameraObjectFollowPos.z += value; // add the values for move the camera

        cameraObjectFollowPos.z = Mathf.Clamp(cameraObjectFollowPos.z, minZoom, maxZoom);
        CameraManager.Instance.CameraObjectToFollow.position = Vector3.MoveTowards(CameraManager.Instance.CameraObjectToFollow.position, cameraObjectFollowPos, zoomSpeed * Time.deltaTime); // Move towards is better for movimentation
    }


    public void ResetZoom(Transform startDragPos, float zoomSpeed = 100f)
    {
        cameraObjectFollowPos = CameraManager.Instance.CameraObjectToFollow.position;
        cameraObjectFollowPos.z -= startDragPos.position.z;

        CameraManager.Instance.CameraObjectToFollow.position = Vector3.Lerp(CameraManager.Instance.CameraObjectToFollow.position, cameraObjectFollowPos, Time.deltaTime * zoomSpeed); // We use lerp because the move is automatic, so we need to control the velocity
    }

    private void OnDestroy()
    {
        inputReader.OnSecondaryTouchContactEvent -= InputReader_OnSecondaryTouchContactEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
        inputReader.OnSecondaryFingerPositionEvent -= InputReader_OnSecondaryFingerPositionEvent;
    }
}
