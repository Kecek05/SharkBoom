using Sortify;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndShoot : MonoBehaviour
{

    [BetterHeader("References")]

    [SerializeField] private Transform spawnPos;
    [SerializeField] private Transform shperePos;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Trajectory trajectory;
    [SerializeField] private Camera playerCam;

    [BetterHeader("Force Settings")]
    [Tooltip("Maximum Force that the Object can go")]
    [SerializeField] private float maxForceMultiplier = 100f;

    [Tooltip("Time to the trajectories get to the final position")] [RangeStep(0.01f, 0.5f, 0.01f)] [SerializeField] private float smoothTime = 0.1f;

    [Tooltip("Value to be add to not need to drag too far from the object")][SerializeField] private float offsetForceMultiplier = 1f;

    private Vector3 velocity = Vector3.zero;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 direction;
    private float distance;

    private float forceMultiplier;
    private bool isDragging = false;


    private void Start()
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        inputReader.OnPrimaryFingerPositionEvent += InputReader_OnPrimaryFingerPositionEvent;
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        if (context.started) // capture the first frame when the touch is pressed
        {
            Ray rayStart = playerCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, out hit) && hit.collider.gameObject == this.gameObject) // compare if the touch hit on the object
            {
                startPos = hit.point; // save the start position of the drag
                isDragging = true;
                trajectory.Show(); // call the function for show dots
            }
        }

        if (context.canceled && isDragging)
        {
            ReleaseDrag();
        }
    }

    private void InputReader_OnPrimaryFingerPositionEvent(InputAction.CallbackContext context)
    {
        if (isDragging)
        {
            Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.forward, startPos); // we create the plane to calculate the Z, because a click is a 2D position

            if (plane.Raycast(ray, out distance))
            {
                endPos = Vector3.SmoothDamp(endPos, ray.GetPoint(distance), ref velocity, smoothTime);

                direction = (startPos - endPos).normalized; // calculate the direction of the drag
                forceMultiplier = Mathf.Pow(Vector3.Distance(startPos, endPos), offsetForceMultiplier);
                Debug.Log($"ForceMultiplier: {forceMultiplier} and Actual Distance: {Vector3.Distance(startPos, endPos)}");
                forceMultiplier = Mathf.Clamp(forceMultiplier, 0, maxForceMultiplier); // clamp the force multiplier
                trajectory.UpdateDots(shperePos.position, direction * forceMultiplier); // update the dots position 
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // just for debug, reset the position of the sphere
        {
            shperePos.position = spawnPos.position;
        }
    }

    private void ReleaseDrag()
    {
        rb.AddForce(direction * forceMultiplier, ForceMode.Impulse);
        isDragging = false;
        trajectory.Hide();
    }

}
