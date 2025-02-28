using Sortify;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndShoot : MonoBehaviour
{
    public event Action OnDragRelease;
    public event Action OnDragStart;

    [BetterHeader("References")]

    [SerializeField] private InputReader inputReader;
    [SerializeField] private Trajectory trajectory;

    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")]
    [SerializeField] private float maxForceMultiplier = 100f;

    [Tooltip("Time to the trajectories get to the final position")] [RangeStep(0.01f, 0.5f, 0.01f)] 
    [SerializeField] private float smoothTime = 0.1f;

    [Tooltip("Value to be add to not need to drag too far from the object")]
    [SerializeField] private float offsetForceMultiplier = 2f;

    [Tooltip("Center position of the drag")]
    [SerializeField]private Transform startDragPos;

    private Vector3 velocity = Vector3.zero; //cache


    private Vector3 endPos;
    private Vector3 direction;
    public Vector3 Direction => direction;


    private float distance;

    private float force;
    public float Force => force;


    private bool isDragging = false;

    private bool canDrag = true;
    public bool CanDrag => canDrag;


    public void Initialize() //Setup
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;

        trajectory.Initialize();
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
                isDragging = true;
                trajectory.Show(); // call the function for show dots
            }

            OnDragStart?.Invoke();
        }

        if (context.canceled && isDragging)
        {
            isDragging = false;
            OnDragRelease?.Invoke();
        }
    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        if(!canDrag) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, startDragPos.position); // we create the plane to calculate the Z, because a click is a 2D position

        if (plane.Raycast(ray, out distance))
        {
            endPos = Vector3.SmoothDamp(endPos, ray.GetPoint(distance), ref velocity, smoothTime);

            direction = (startDragPos.position - endPos).normalized; // calculate the direction of the drag

            force = Mathf.Pow(Vector3.Distance(startDragPos.position, endPos), offsetForceMultiplier); //Calculate the force exponentially
            force = Mathf.Clamp(force, 0, maxForceMultiplier);

            Debug.Log($"ForceMultiplier: {force} and Actual Distance: {Vector3.Distance(startDragPos.position, endPos)}");


            trajectory.UpdateDots(transform.position, direction * force); // update the dots position 
        }
    }


    public void ReleaseDrag()
    {
        //rb.AddForce(direction * forceMultiplier, ForceMode.Impulse);
        isDragging = false;
        trajectory.Hide();
    }


    public void SetCanDrag(bool value)
    {
        canDrag = value;
    }

    private void OnDestroy()
    {
        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent -= InputReader_OnPrimaryFingerPositionEvent;
    }
}
