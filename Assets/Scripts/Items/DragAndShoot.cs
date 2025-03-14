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
    [SerializeField] protected Trajectory trajectory;
    [SerializeField] protected InputReader inputReader;
    [SerializeField] protected GameObject areaOfStartDrag;
    [Tooltip("Center position of the drag")]
    [SerializeField] protected Transform startTrajectoryPos;
    [SerializeField] protected LayerMask touchLayer;



    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")][RangeStep(1f, 50f, 1f)]
    [SerializeField] protected float maxForce = 50f;

    [Tooltip("Minimum Force that the Object can go")][RangeStep(1f, 50f, 1f)]
    [SerializeField] private float minForce = 1f;

    [Tooltip("Value to be add to not need to drag too far from the object")]
    [RangeStep(1.1f, 5f, 0.2f)]
    [SerializeField] protected float offsetForce = 0.1f;

    [BetterHeader("Zoom Settings")]
    [Tooltip("Speed of the Zoom")]
    [SerializeField] protected float zoomDragSpeed;

    [Tooltip("Amount of zoom to change")]
    [SerializeField] protected float zoomAmountToChange = 5f;

    [Tooltip("Increase the force of zoom")]
    [SerializeField] protected float zoomMultiplier = 7f;


    protected Vector3 endPosDrag;
    protected Vector3 directionOfDrag;
    protected float dragForce;
    protected float lastDragDistance;
    [SerializeField] protected float dragChangedOffset;

    [Tooltip("Will only detect the distance if exceeds threshold")]
    [SerializeField] private float detectDistanceThreshold = 2f;
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


    //Publics

    public Vector3 DirectionOfDrag => directionOfDrag;
    public Vector3 EndPosDrag => endPosDrag;

    public float DragDistance => dragDistance;
    public float LastDragDistance => lastDragDistance;

    public float DragForce => dragForce;
    public bool CanDrag => canDrag;
    public float MaxForceMultiplier => maxForce;


    protected Rigidbody selectedRb;
    public Rigidbody SelectedRb => selectedRb; //DEBUG


    private bool canCancelDrag = false;

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
                    startZoomPos = CameraManager.Instance.CameraObjectToFollow;

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

            dragForce = dragDistance * offsetForce; //Calculate the force linearly
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
                    CameraManager.Instance.CameraZoom.ChangeZoom(-zoomAmountToChange, zoomDragSpeed);
                    Debug.Log("Force is increasing");
                }
                else if (roundedDragDistance < roundedLastDragDistance)
                {
                    //Force is decreasing

                    CameraManager.Instance.CameraZoom.ChangeZoom(zoomAmountToChange, zoomDragSpeed);

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
            OnDragCancelable?.Invoke(true);
        }
        else
        {
            OnDragCancelable?.Invoke(false);
        }
    }


    public void ResetDrag()
    {
        // Reset the dots position
        trajectory.UpdateDots(startTrajectoryPos.position, directionOfDrag * minForce, selectedRb);
        SetIsDragging(false);
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

    private void SetCanCancelDrag(bool value)
    {
        canCancelDrag = value;
    }
    public void SetIsDragging(bool dragging)
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
