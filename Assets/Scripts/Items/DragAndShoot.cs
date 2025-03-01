using Sortify;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndShoot : MonoBehaviour
{
    public event Action OnDragRelease;
    public event Action OnDragStart;

    [BetterHeader("References")]

    [SerializeField] private Trajectory trajectory;

    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")] [RangeStep(1f, 100f, 1f)]
    [SerializeField] private float maxForceMultiplier = 50f;

    [Tooltip("Minimum Force that the Object can go")] [RangeStep(1f, 100f, 1f)]
    [SerializeField] private float minForceMultiplier = 1f;

    [Tooltip("Time to the trajectories get to the final position")] [RangeStep(0.01f, 0.5f, 0.01f)] 
    [SerializeField] private float smoothTime = 0.1f;

    [Tooltip("Value to be add to not need to drag too far from the object")]
    [RangeStep(1.1f, 5f, 0.2f)]
    [SerializeField] private float offsetForceMultiplier = 0.1f;

    [Tooltip("Center position of the drag")]
    [SerializeField]private Transform startDragPos;

    /// <summary>
    /// Vai receber um input das propriedades o item, como: Massa. Reajustar  a trajetória baseado nos valores
    /// </summary>

    private Vector3 velocity = Vector3.zero; //cache


    private Vector3 endPos;
    private Vector3 direction;
    public Vector3 Direction => direction;

    private Plane plane; //Cache for the clicks
    private bool isShowingDots; //Cache for show dots

    private float distance;

    private float lastForce; // used for zoomOut
    private float force;
    public float Force => force;


    private bool isDragging = false;

    private bool canDrag = true;
    public bool CanDrag => canDrag;

    [SerializeField] private float zoomThreshold = 0.2f;

    public void Initialize() //Setup
    {
        CameraManager.Instance.InputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        CameraManager.Instance.InputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;

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
                CameraManager.Instance.SetCameraState(CameraManager.CameraState.Dragging);

                plane = new Plane(Vector3.forward, startDragPos.position); // we create the plane to calculate the Z, because a click is a 2D position
                isShowingDots = false;
                lastForce = 0f;

                isDragging = true;
                OnDragStart?.Invoke();
            }
        }

        if (context.canceled && isDragging)
        {
            isDragging = false;
            OnDragRelease?.Invoke();
            CameraManager.Instance.SetCameraState(CameraManager.CameraState.Default);
        }
    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {

        if(!canDrag || !isDragging) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //CHANGE TO CONTEXT
       

        if (plane.Raycast(ray, out distance))
        {
            endPos = Vector3.SmoothDamp(endPos, ray.GetPoint(distance), ref velocity, smoothTime);

            direction = (startDragPos.position - endPos).normalized; // calculate the direction of the drag

            //force = Mathf.Pow(Vector3.Distance(startDragPos.position, endPos), offsetForceMultiplier); //Calculate the force exponentially
            force = Vector3.Distance(startDragPos.position, endPos) * offsetForceMultiplier; //Calculate the force linearly
            force = Mathf.Clamp(force, minForceMultiplier, maxForceMultiplier);
            // Debug.Log($"ForceMultiplier: {force} and Actual Distance: {Vector3.Distance(startDragPos.position, endPos)}"); cost much perfomance


            trajectory.UpdateDots(transform.position, direction * force); // update the dots position 

            if (Mathf.Abs(lastForce - force) > zoomThreshold)
            {
                if (lastForce > force)
                {
                    CameraManager.Instance.CameraZoom.AdaptZoomInDrag(force);
                }
                else if (lastForce < force)
                {
                    CameraManager.Instance.CameraZoom.AdaptZoomOutDrag(force);
                }
            }

            lastForce = force;
            // Debug.Log($"Last force: {lastForce} and force {force}");

            if (!isShowingDots)
            {
                trajectory.Show(); // call the function for show dots
                isShowingDots = true;
            }
        }
    }


    public void ReleaseDrag()
    {
        isDragging = false;
        CameraManager.Instance.CameraZoom.ResetZoom();
        trajectory.Hide();
    }

    public void ResetDragPos()
    {
        // Reset the dots position
        trajectory.UpdateDots(transform.position, direction * minForceMultiplier);
        ReleaseDrag();
    }

    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    private void OnDestroy()
    {
        CameraManager.Instance.InputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        CameraManager.Instance.InputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
    }
}
