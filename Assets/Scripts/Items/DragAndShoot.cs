using QFSW.QC;
using Sortify;
using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndShoot : NetworkBehaviour
{
    
    public event Action OnDragRelease;
    public event Action OnDragStart;

    [BetterHeader("References")]

    [SerializeField] protected Trajectory trajectory;
    [SerializeField] protected InputReader inputReader;
    [Tooltip("Center position of the drag")]
    [SerializeField] protected Transform startDragPos;
    [SerializeField] protected LayerMask touchLayer;


    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")] [RangeStep(1f, 50f, 1f)]
    [SerializeField] protected float maxForceMultiplier = 50f;

    [Tooltip("Minimum Force that the Object can go")] [RangeStep(1f, 50f, 1f)]
    [SerializeField] protected float minForceMultiplier = 1f;

    [Tooltip("Value to be add to not need to drag too far from the object")]
    [RangeStep(1.1f, 5f, 0.2f)]
    [SerializeField] protected float offsetForceMultiplier = 0.1f;

    [BetterHeader("Zoom Settings")]
    [Tooltip("Time to the drag updtae the zoom")] 
    [SerializeField] protected float zoomDragSpeed;

    [Tooltip("Increase the force of zoom")] 
    [SerializeField] protected float zoomMultiplier = 7f;


    protected Vector3 endPosDrag;
    protected Vector3 directionOfDrag;
    protected float dragForce;
    protected bool isDragging = false;
    protected bool canDrag = false;
    protected float dragDistance;


    protected Transform startZoomPos; // store the original zoom position
    protected float zoomForce; // current force of zoom
    protected bool isZoomIncreasing; // bool for check if the force is decreasing or increasing and allow the zoom
    protected float lastZoomForce = 0f; // Store the last zoom force
    protected float checkMovementInterval = 0.001f; // control the time between checks of the zoom force, turn the difference bigger
    protected float lastCheckTime = 0f; // control the time between checks

    protected Plane plane; // Cache for the clicks
    protected float outDistancePlane; // store the distance of the plane and screen

    protected bool isShowingDots; //Cache for show dots


    public Vector3 DirectionOfDrag => directionOfDrag;
    public float DragForce => dragForce;
    public bool CanDrag => canDrag;


    protected Rigidbody selectedRb;

    public Rigidbody SelectedRb => selectedRb; //DEBUG

    private bool isCancelingDrag = false;
    private bool canCancelDrag = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;

        trajectory.Initialize(startDragPos);
    }

    public void SetDragAndShoot(Rigidbody rb)
    {
        selectedRb = rb;
        SetCanDrag(true);
    }

    protected void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        if (!canDrag) return;

        if (context.started) // capture the first frame when the touch is pressed
        {
 
            Ray rayStart = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, out hit, Mathf.Infinity, touchLayer)) // compare if the touch hit on the object
            {
                if(hit.collider.gameObject == this.gameObject)
                {
                    //Start Dragging
                    isCancelingDrag = false;
                    canCancelDrag = false;
                    trajectory.SetSimulation(true);
                    CameraManager.Instance.SetCameraState(CameraManager.CameraState.Dragging);
                    startZoomPos = CameraManager.Instance.CameraObjectToFollow;

                    plane = new Plane(Vector3.forward, startDragPos.position); // we create the plane to calculate the Z, because a click is a 2D position

                    SetIsShowingDots(false);
                    SetIsDragging(true);
                    OnDragStart?.Invoke();
                }
            }
        }

        if (context.canceled && isDragging && !isCancelingDrag)
        { 
            SetIsDragging(false);
            trajectory.SetSimulation(false);
            OnDragRelease?.Invoke();
            CameraManager.Instance.SetCameraState(CameraManager.CameraState.Default);
        }
    }

    protected void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {

        if(!canDrag || !isDragging || selectedRb == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //CHANGE TO CONTEXT
       

        if (plane.Raycast(ray, out outDistancePlane) && Input.touchCount == 1 && !isCancelingDrag) // this input touch count is a check for avoid the player bug if accidentally touch the screen with two fingers
        {
            endPosDrag = ray.GetPoint(outDistancePlane); // get the position of the click instantaneously
            directionOfDrag = (startDragPos.position - endPosDrag).normalized; // calculate the direction of the drag on Vector3
            dragDistance = Vector3.Distance(startDragPos.position, endPosDrag); // calculate the distance of the drag on float

            dragForce = dragDistance * offsetForceMultiplier; //Calculate the force linearly
            dragForce = Mathf.Clamp(dragForce, minForceMultiplier, maxForceMultiplier);

            trajectory.UpdateDots(transform.position, directionOfDrag * dragForce, selectedRb); // update the dots position 

            if (Time.time - lastCheckTime >= checkMovementInterval)
            {
                zoomForce = dragForce * zoomMultiplier * dragDistance;
                isZoomIncreasing = zoomForce > lastZoomForce; // Check is zoomForce is increasing

                if (dragDistance > 2f)
                {
                    canCancelDrag = true;
                }

                if (isZoomIncreasing)
                {
                    // zoom out
                    CameraManager.Instance.CameraZoom.ChangeZoom(-5f, zoomDragSpeed);
                }
                else
                {
                    // zoom in
                    CameraManager.Instance.CameraZoom.ChangeZoom(5f, zoomDragSpeed);

                    if(dragDistance < 0.9f && canCancelDrag)
                    {
                        isCancelingDrag = true;
                        isDragging = false;
                        trajectory.Hide();
                        SetIsShowingDots(false);
                    }
                }

                lastZoomForce = zoomForce; // Update the last zoom force
                lastCheckTime = Time.time; // Update the last check time
            }

            if (!isShowingDots && !isCancelingDrag)
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
        trajectory.UpdateDots(transform.position, directionOfDrag * minForceMultiplier, selectedRb);

        ReleaseDrag();

    }

    public void ReleaseDrag()
    {
        SetIsDragging(false);
        SetIsShowingDots(false);
        trajectory.Hide();
    }


    public void TurnOffDrag()
    {
        SetCanDrag(false);
        SetIsDragging(false);
    }

    public void TurnOnDrag()
    {
        SetCanDrag(true);
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    public void SetIsDragging(bool value)
    {
        isDragging = value;
    }

    public void SetIsShowingDots(bool value)
    {
        isShowingDots = value;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;

    }


}
