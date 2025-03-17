using QFSW.QC;
using Sortify;
using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndShoot : NetworkBehaviour
{
    /// <summary>
    /// Event that is called when the drag is released, the object is launched
    /// </summary>
    public event Action OnDragRelease;

    /// <summary>
    /// Event that is called when the drag is starting, finger is pressed
    /// </summary>
    public event Action OnDragStart;

    /// <summary>
    /// Event that is called when the drag is changing position, finger changed pos
    /// </summary>
    public event Action OnDragChange;

    /// <summary>
    /// Event that is called when the drag is cancelable
    /// </summary>
    public event Action<bool> OnDragCancelable;

    [BetterHeader("References")]
    [SerializeField] protected Player player;
    [SerializeField] private Trajectory trajectory;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject areaOfStartDrag;
    [Tooltip("Center position of the drag")]
    [SerializeField] private Transform startTrajectoryPos;
    [SerializeField] private LayerMask touchLayer;
    [SerializeField] private CameraManager cameraManager;


    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")][RangeStep(1f, 50f, 1f)]
    [SerializeField] private float maxForce = 50f;

    [Tooltip("Minimum Force that the Object can go")][RangeStep(1f, 50f, 1f)]
    [SerializeField] private float minForce = 1f;

    [Tooltip("Value to be add to not need to drag too far from the object")]
    [RangeStep(1.1f, 5f, 0.2f)]
    [SerializeField] private float forceAddMultiplier = 0.1f;

    [BetterHeader("Zoom Settings")]
    [Tooltip("Speed of the Zoom")]
    [SerializeField] private float zoomDragSpeed;

    [Tooltip("Amount of zoom to change")]
    [SerializeField] private float zoomAmountToChange = 5f;

    [Tooltip("Will only detect the distance if exceeds threshold")]
    [SerializeField] private int detectDistanceThreshold = 2;

    private Vector3 endPosDrag;
    private Vector3 directionOfDrag;
    private float dragForce;
    private float lastDragDistance;

    private bool isDragging = false;
    private bool canDrag = false;
    private float dragDistance;


    private Transform startZoomPos; // store the original zoom position
    private float zoomForce; // current force of zoom
    private bool isZoomIncreasing; // bool for check if the force is decreasing or increasing and allow the zoom
    private float lastZoomForce = 0f; // Store the last zoom force
    private float checkMovementInterval = 0.001f; // control the time between checks of the zoom force, turn the difference bigger
    private float lastCheckTime = 0f; // control the time between checks

    private Plane plane; // Cache for the clicks
    private float outDistancePlane; // store the distance of the plane and screen
    private bool canCancelDrag = false;

    protected Rigidbody selectedRb;

    //Publics

    public Vector3 DirectionOfDrag => directionOfDrag;
    public Vector3 EndPosDrag => endPosDrag;

    public float DragDistance => dragDistance;
    public float LastDragDistance => lastDragDistance;

    public float DragForce => dragForce;
    public bool CanDrag => canDrag;
    public float MaxForceMultiplier => maxForce;


    public Rigidbody SelectedRb => selectedRb; //DEBUG



    [Tooltip("Distance to cancel the drag")]
    [SerializeField] private float canceDragDistance = 0.9f;

    private float deltaDetectDistance = 0f; //cache
    private int roundedLastDragDistance = 0; //cache
    private int roundedDragDistance = 0; //cache


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;

        trajectory.Initialize(startTrajectoryPos);
    }

    public void SetDragRb(Rigidbody rb)
    {
        selectedRb = rb;
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
                if (hit.collider.gameObject == areaOfStartDrag)
                {
                    //Start Dragging
                    SetCanCancelDrag(false);
                    trajectory.SetSimulation(true);
                    startZoomPos = cameraManager.CameraObjectToFollow;

                    plane = new Plane(Vector3.forward, Input.mousePosition); // we create the plane to calculate the Z, because a click is a 2D position

                    SetIsDragging(true);
                    OnDragStart?.Invoke();
                }
            }
        }

        if (context.canceled && isDragging)
        {
            //released the finger
            if (canCancelDrag)
            {
                //reset all
                SetCanCancelDrag(false);
                SetIsDragging(false);
                player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.idleMyTurnState);
                OnDragCancelable?.Invoke(false);
                return;
            } else
            {
                //shoot
                SetIsDragging(false);
                trajectory.SetSimulation(false);
                OnDragRelease?.Invoke();
            }


        }
    }

    protected void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {

        if (!canDrag || !isDragging || selectedRb == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //CHANGE TO CONTEXT


        if (plane.Raycast(ray, out outDistancePlane) && Input.touchCount == 1) // this input touch count is a check for avoid the player bug if accidentally touch the screen with two fingers
        {
            endPosDrag = ray.GetPoint(outDistancePlane); // get the position of the click instantaneously
            directionOfDrag = (startTrajectoryPos.position - endPosDrag).normalized; // calculate the direction of the drag on Vector3
            dragDistance = Vector3.Distance(startTrajectoryPos.position, endPosDrag); // calculate the distance of the drag on float

            dragForce = dragDistance * forceAddMultiplier; //Calculate the force linearly
            dragForce = Mathf.Clamp(dragForce, minForce, maxForce);



            trajectory.UpdateDots(startTrajectoryPos.position, directionOfDrag * dragForce, selectedRb); // update the dots position 

            OnDragChange?.Invoke();

            CheckCancelDrag();

            // Convert the drag distances to an absolute, rounded integer.
            roundedLastDragDistance = Mathf.Abs(Mathf.RoundToInt(lastDragDistance));
            roundedDragDistance = Mathf.Abs(Mathf.RoundToInt(dragDistance));


            deltaDetectDistance = Mathf.Abs(roundedDragDistance - roundedLastDragDistance);


            if (deltaDetectDistance >= detectDistanceThreshold)
            {
                //Detected a change in distance

                if (roundedDragDistance > roundedLastDragDistance)
                {
                    //Force is increasing
                    cameraManager.CameraZoom.ChangeZoom(-zoomAmountToChange, zoomDragSpeed);
                    Debug.Log("Force is increasing");
                }
                else if (roundedDragDistance < roundedLastDragDistance)
                {
                    //Force is decreasing

                    cameraManager.CameraZoom.ChangeZoom(zoomAmountToChange, zoomDragSpeed);

                    Debug.Log("Force is decreasing");
                }


                lastDragDistance = dragDistance; //only update if changed
            }
            else
            {
                //Not detected a change in distance
            }
        }
    }

    private void CheckCancelDrag()
    {
        if (Mathf.Abs(dragDistance) <= canceDragDistance)
        {
            SetCanCancelDrag(true);
            trajectory.Hide();
            OnDragCancelable?.Invoke(true);
        }
        else
        {
            SetCanCancelDrag(false);
            trajectory.Show();
            OnDragCancelable?.Invoke(false);
        }
    }


    protected void ResetDrag()
    {
        // Reset the dots position
        trajectory.UpdateDots(startTrajectoryPos.position, directionOfDrag * minForce, selectedRb);
        SetIsDragging(false);
    }


    protected void TurnOffDrag()
    {
        SetCanDrag(false);
        SetIsDragging(false);
        SetCanCancelDrag(false);
    }

    protected void TurnOnDrag()
    {
        SetCanDrag(true);
    }

    private void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    private void SetCanCancelDrag(bool value)
    {
        canCancelDrag = value;
    }

    private void SetIsDragging(bool dragging)
    {
        isDragging = dragging;

        if(dragging)
        {
            trajectory.Show();
        } else
        {
            trajectory.Hide();
        }
    }

    public float GetAngle()
    {
        return Mathf.Atan2(directionOfDrag.y, directionOfDrag.x) * Mathf.Rad2Deg;
    }

    public float GetForcePercentage()
    {
        return (dragForce / maxForce) * 100f;
    }

    public Vector3 GetOpositeFingerPos()
    {
        return (startTrajectoryPos.position - endPosDrag) + startTrajectoryPos.position;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
    }
}
