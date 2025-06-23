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
    [SerializeField] private float dotSpacingOffsetMultiply;
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
                dotsList[i].gameObject.SetActive(true);
                // dotsList[i].gameObject.SetActive(i < targetActiveDots);
            }

            lastForceApplied = percentForceApplied;
        }

        for (int i = 0; i < dotsNumber && i < trajectoryPoints.Count; i++)
        {
            dotsList[i].position = trajectoryPoints[i];
            dotsList[i].localPosition = new Vector3(0f, dotsList[i].localPosition.y * dotSpacingOffsetMultiply, dotsList[i].localPosition.z * dotSpacingOffsetMultiply);
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
            Vector3 newPos = new Vector3(0f, pos.y, pos.z);
            GameObject cube = Instantiate(cubeDebug, newPos, Quaternion.identity);
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
    
    private void SimulateTrajectory(Vector3 objectPos, float dragForce, Vector3 directionOfDragNormalized, Rigidbody rb)
    {
        if (!isSimulating) return;
        ghostPos.Clear();
        Physics.simulationMode = SimulationMode.Script;

        GameObject ghostObj = new GameObject("Ghost");
        Rigidbody ghost = ghostObj.AddComponent<Rigidbody>();

        ghost.mass = rb.mass;
        ghost.linearDamping = rb.linearDamping;
        ghost.angularDamping = rb.angularDamping;
        ghost.useGravity = rb.useGravity;
        ghost.position = objectPos;
        ghost.isKinematic = false;
        ghost.AddForce(directionOfDragNormalized * dragForce, ForceMode.Impulse);

        trajectoryPoints.Clear();

        for (int i = 0; i < dotsNumber; i++)
        {
            float timeStep = Time.fixedDeltaTime;
            Physics.Simulate(timeStep);
            ghostPos.Add(ghostObj.transform.position);
            trajectoryPoints.Add(ghost.position);
        }

        Physics.simulationMode = SimulationMode.FixedUpdate;
        Destroy(ghostObj);

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
