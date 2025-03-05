using QFSW.QC;
using Sortify;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndShoot : MonoBehaviour
{
    
    public event Action OnDragRelease;
    public event Action OnDragStart;

    [BetterHeader("References")]

    [SerializeField] private Trajectory trajectory;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Player player;
    [Tooltip("Center position of the drag")]
    [SerializeField] private Transform startDragPos;


    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")] [RangeStep(1f, 50f, 1f)]
    [SerializeField] private float maxForceMultiplier = 50f;

    [Tooltip("Minimum Force that the Object can go")] [RangeStep(1f, 50f, 1f)]
    [SerializeField] private float minForceMultiplier = 1f;

    [Tooltip("Value to be add to not need to drag too far from the object")]
    [RangeStep(1.1f, 5f, 0.2f)]
    [SerializeField] private float offsetForceMultiplier = 0.1f;

    [BetterHeader("Zoom Settings")]
    [Tooltip("Time to the drag updtae the zoom")] 
    [SerializeField] private float zoomDragSpeed;

    [Tooltip("Increase the force of zoom")] 
    [SerializeField] private float zoomMultiplier = 7f;


    private Vector3 endPosDrag;
    private Vector3 directionOfDrag;
    private float dragForce;
    private bool isDragging = false;
    private bool canDrag = true;
    private float dragDistance;


    private Transform startZoomPos; // store the original zoom position
    private float zoomForce; // current force of zoom
    private bool isZoomIncreasing; // bool for check if the force is decreasing or increasing and allow the zoom
    private float lastZoomForce = 0f; // Store the last zoom force
    private float checkMovementInterval = 0.001f; // control the time between checks of the zoom force, turn the difference bigger
    private float lastCheckTime = 0f; // control the time between checks

    private Plane plane; // Cache for the clicks
    private float outDistancePlane; // store the distance of the plane and screen

    private bool isShowingDots; //Cache for show dots


    public Vector3 DirectionOfDrag => directionOfDrag;
    public float DragForce => dragForce;
    public bool CanDrag => canDrag;


    public void Initialize() //Setup
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;

        player.OnPlayerReady += Player_OnPlayerReady; ;
        GameFlowManager.OnRoundStarted += GameFlowManager_OnRoundGoing;
        GameFlowManager.OnRoundPreparing += GameFlowManager_OnRoundPreparing;
        trajectory.Initialize(startDragPos);
    }


    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        if (!canDrag) return;

        if (context.started) // capture the first frame when the touch is pressed
        {
            Ray rayStart = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, out hit) && hit.collider.gameObject == this.gameObject) // compare if the touch hit on the object
            {
                //Start Dragging
                trajectory.SetSimulation(true);
                CameraManager.Instance.SetCameraState(CameraManager.CameraState.Dragging);
                startZoomPos = CameraManager.Instance.CameraObjectToFollow;

                plane = new Plane(Vector3.forward, startDragPos.position); // we create the plane to calculate the Z, because a click is a 2D position
                
                SetIsShowingDots(false);
                SetIsDragging(true);
                OnDragStart?.Invoke();
            }
        }

        if (context.canceled && isDragging)
        {
            SetIsDragging(false);
            trajectory.SetSimulation(false);
            OnDragRelease?.Invoke();
            CameraManager.Instance.SetCameraState(CameraManager.CameraState.Default);
        }
    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {

        if(!canDrag || !isDragging) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //CHANGE TO CONTEXT
       

        if (plane.Raycast(ray, out outDistancePlane))
        {
            endPosDrag = ray.GetPoint(outDistancePlane); // get the position of the click instantaneously
            directionOfDrag = (startDragPos.position - endPosDrag).normalized; // calculate the direction of the drag on Vector3
            dragDistance = Vector3.Distance(startDragPos.position, endPosDrag); // calculate the distance of the drag on float

            dragForce = dragDistance * offsetForceMultiplier; //Calculate the force linearly
            dragForce = Mathf.Clamp(dragForce, minForceMultiplier, maxForceMultiplier);

            trajectory.UpdateDots(transform.position, directionOfDrag * dragForce, player.GetSelectedItemSO()); // update the dots position 

            if (Time.time - lastCheckTime >= checkMovementInterval)
            {
                zoomForce = dragForce * zoomMultiplier * dragDistance;
                isZoomIncreasing = zoomForce > lastZoomForce; // Check is zoomForce is increasing

                if (isZoomIncreasing)
                {
                    // zoom out
                    CameraManager.Instance.CameraZoom.ChangeZoom(-5f, zoomDragSpeed);
                }
                else
                {
                    // zoom in
                    CameraManager.Instance.CameraZoom.ChangeZoom(5f, zoomDragSpeed);
                }

                lastZoomForce = zoomForce; // Update the last zoom force
                lastCheckTime = Time.time; // Update the last check time
            }

            if (!isShowingDots)
            {
                trajectory.Show(); // call the function for show dots
                SetIsShowingDots(true);
            }
        }
    }

    //DEBUG

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            DragMouseDebug();
        }
    }
    private void DragMouseDebug()
    {
        trajectory.SetSimulation(true);

        plane = new Plane(Vector3.forward, startDragPos.position); // we create the plane to calculate the Z, because a click is a 2D position

        SetIsShowingDots(false);
        SetIsDragging(true);
        OnDragStart?.Invoke();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //CHANGE TO CONTEXT


        if (plane.Raycast(ray, out outDistancePlane))
        {
            endPosDrag = ray.GetPoint(outDistancePlane); // get the position of the click instantaneously
            directionOfDrag = (startDragPos.position - endPosDrag).normalized; // calculate the direction of the drag on Vector3
            dragDistance = Vector3.Distance(startDragPos.position, endPosDrag); // calculate the distance of the drag on float

            dragForce = dragDistance * offsetForceMultiplier; //Calculate the force linearly
            dragForce = Mathf.Clamp(dragForce, minForceMultiplier, maxForceMultiplier);

            trajectory.UpdateDots(transform.position, directionOfDrag * dragForce, player.GetSelectedItemSO()); // update the dots position 

            if (!isShowingDots)
            {
                trajectory.Show(); // call the function for show dots
                SetIsShowingDots(true);
            }
        }
    }

    public void ResetDrag()
    {
        // Reset the dots position
        //CameraManager.Instance.CameraZoom.ResetZoom(startZoomPos); // Reset the zoom for start position
        trajectory.UpdateDots(transform.position, directionOfDrag * minForceMultiplier, player.GetSelectedItemSO());
        ReleaseDrag();

    }

    public void ReleaseDrag()
    {
        SetIsDragging(false);
        SetIsShowingDots(false);
        trajectory.Hide();
    }


    private void Player_OnPlayerReady()
    {
        //Turn Off
        TurnOffDrag();
    }

    private void GameFlowManager_OnRoundGoing()
    {
        //Hide Dots, already Off
        ResetDrag();
    }

    private void GameFlowManager_OnRoundPreparing()
    {
        //Back to normal
        TurnOnDrag();
    }

    private void TurnOffDrag()
    {
        SetCanDrag(false);
        SetIsDragging(false);
    }

    private void TurnOnDrag()
    {
        SetCanDrag(true);
    }


    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    private void SetIsDragging(bool value)
    {
        isDragging = value;
    }

    private void SetIsShowingDots(bool value)
    {
        isShowingDots = value;
    }

    private void OnDestroy()
    {
        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
    }
}
