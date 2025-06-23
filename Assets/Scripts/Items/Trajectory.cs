using Sortify;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject dotPrefab2D;
    [SerializeField] private DragAndShoot dragAndShoot;

    [BetterHeader("Settings")]
    [Range(3, 50)]
    [SerializeField] private int dotsNumber;
    [RangeStep(1f, 10f, 0.1f)]
    [SerializeField] private float forcePercentChangeThreshold = 1f;

    private Transform[] dotsList;
    private GameObject dotsParent;

    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private bool isSimulating;

    
    
    private float lastForceApplied;
    private float currentForcePercent;
    private float previousForcePercent;
    private float diff;

    public void Initialize(Transform dotsParentTransform)
    {
        dotsParent = dotsParentTransform.gameObject;
        PrepareDots();
        Hide();
    }

    private void PrepareDots()
    {
        dotsList = new Transform[dotsNumber];

        for (int i = 0; i < dotsNumber; i++)
        {
            dotsList[i] = Instantiate(dotPrefab2D, dotsParent.transform).transform;
            dotsList[i].position = dotsParent.transform.position; // set dots pos to parent
            dotsList[i].gameObject.SetActive(false);
        }
    }

    public void UpdateDots(Vector3 objectPos, float dragForce, Vector3 directionOfDragNormalized, Rigidbody rb, float percentForceApplied) 
    {
        trajectoryPoints.Add(objectPos);
        SimulateTrajectory(objectPos, dragForce, directionOfDragNormalized, rb);

        currentForcePercent = percentForceApplied;
        previousForcePercent = lastForceApplied;
        diff = currentForcePercent - previousForcePercent;

        float percentForceAppliedInDecimals = percentForceApplied / 200;
        
        // Debug.Log($"TRAJECTORY - Current Force: {currentForce} - Previous Force: {previousForce} - Diff: {diff}");
        if (Mathf.Abs(diff) >= forcePercentChangeThreshold)
        {
            // int targetActiveDots = Mathf.Clamp(Mathf.RoundToInt((currentForce / maxForce) * dotsNumber), 0, dotsNumber);
            int targetActiveDots = Mathf.Clamp(Mathf.RoundToInt(dotsNumber * percentForceAppliedInDecimals), 0, dotsNumber);
            // Debug.Log($"TRAJECTORY - Target Active: {targetActiveDots} - Current Force: {currentForce} - Max Force: {maxForce}");
            for (int i = 0; i < dotsNumber; i++)
            {
                // dotsList[i].gameObject.SetActive(true);
                dotsList[i].gameObject.SetActive(i < targetActiveDots);
            }

            lastForceApplied = percentForceApplied;
        }

        for (int i = 0; i < dotsNumber && i < trajectoryPoints.Count; i++)
        {
            dotsList[i].position = trajectoryPoints[i];
            dotsList[i].localPosition = new Vector3(0f, dotsList[i].localPosition.y, dotsList[i].localPosition.z);
        }
    }

    public GameObject cubeDebug;
    
    public List<Vector3> ghostPos = new List<Vector3>();
    public List<GameObject> cubes = new List<GameObject>();
    
    [Button("SpawnCubes")]
    public void SpawnCubes()
    {
        foreach (Vector3 pos in ghostPos)
        {
            GameObject cube = Instantiate(cubeDebug, pos, Quaternion.identity);
            cubes.Add(cube);
        }
    }

    [Button("Destroy Cubes")]
    public void DestroyCubes()
    {
        foreach (GameObject cube in cubes)
        {
            Destroy(cube);
        }
        cubes.Clear();
    }
    
    private void SimulateTrajectory(Vector3 objectPos, float dragForce, Vector2 directionOfDragNormalized, Rigidbody rb)
    {
        if (!isSimulating) return;
        ghostPos.Clear();
        Physics.simulationMode = SimulationMode.Script;

        GameObject ghostObj = new GameObject("Ghost");
        Rigidbody ghost = ghostObj.AddComponent<Rigidbody>();

        // Match transform
        ghost.position = objectPos;
        ghost.rotation = rb.rotation;

        // Copy Rigidbody state from original
        CopyRigidbodyState(rb, ghost);

        // Apply impulse after copying
        ghost.AddForce(directionOfDragNormalized * dragForce, ForceMode.Impulse);

        trajectoryPoints.Clear();

        for (int i = 0; i < dotsNumber; i++)
        {
            float timeStep = Time.fixedDeltaTime;
            Physics.Simulate(timeStep);
            Physics.Simulate(timeStep);
            Physics.Simulate(timeStep);
            Physics.Simulate(timeStep);
            ghostPos.Add(ghostObj.transform.position);
            trajectoryPoints.Add(ghost.position);
        }

        Physics.simulationMode = SimulationMode.FixedUpdate;
        Destroy(ghostObj);

    }
    
    private void CopyRigidbodyState(Rigidbody source, Rigidbody clone)
    {
        // Core physical properties
        clone.mass = source.mass;
        clone.useGravity = source.useGravity;
        clone.isKinematic = source.isKinematic;
        clone.interpolation = source.interpolation;
        clone.collisionDetectionMode = source.collisionDetectionMode;
        clone.constraints = source.constraints;

        // Unity 6-specific damping
        clone.linearDamping = source.linearDamping;
        clone.angularDamping = source.angularDamping;

        // Velocity and spin
        clone.linearVelocity = source.linearVelocity;
        clone.angularVelocity = source.angularVelocity;

        // Inertia and center of mass (optional but improves realism)
        clone.ResetCenterOfMass();
        clone.ResetInertiaTensor();
        clone.centerOfMass = source.centerOfMass;
        clone.inertiaTensor = source.inertiaTensor;
        clone.inertiaTensorRotation = source.inertiaTensorRotation;

        // Copy sleep/wake state
        if (source.IsSleeping())
            clone.Sleep();
        else
            clone.WakeUp();
    }


    public void Show()
    {
        if(!dotsParent.activeSelf)
            dotsParent.SetActive(true);
    }

    public void Hide()
    {
        if (dotsParent.activeSelf)
            dotsParent.SetActive(false);
    }

    public void SetSimulation(bool _isSimulating)
    {
        isSimulating = _isSimulating;
    }
}
