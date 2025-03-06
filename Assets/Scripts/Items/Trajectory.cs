using Sortify;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private DragAndShoot dragAndShoot;

    [BetterHeader("Settings")]
    [Range(3, 50)]
    [SerializeField] private int dotsNumber;
    [RangeStep(0.01f, 1f, 0.01f)]
    [SerializeField] private float dotSpacing;

    private Vector3 dotPos;
    private float timeStamp; // Position of dots along the trajectory
    private Transform[] dotsList;
    private GameObject dotsParent;
    private Vector3 adjustedForce;
    private Vector3 adjustedForceDamping;
    private float time; // current time of the dots

    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private bool isSimulating;
    

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
            dotsList[i] = Instantiate(dotPrefab, dotsParent.transform).transform; // Create dots based on the number of dots variable
            dotsList[i].position = dotsParent.transform.position; // set the dots position to the parent position (in player)
        }
    }

    public void UpdateDots(Vector3 objectPos, Vector3 forceApplied, Rigidbody rb) 
    {
        trajectoryPoints.Add(objectPos);
        SimulateTrajectory(objectPos, forceApplied, rb);

        for (int i = 0; i < dotsNumber && i < trajectoryPoints.Count; i++)
        {
            dotsList[i].position = trajectoryPoints[i];
        }
    }

    private void SimulateTrajectory(Vector3 objectPos, Vector3 forceApplied, Rigidbody rb)
    {
        if (!isSimulating) return;

        Physics.simulationMode = SimulationMode.Script;

        GameObject ghostObj = new GameObject("Ghost");
        Rigidbody ghost = ghostObj.AddComponent<Rigidbody>();

        ghost.mass = rb.mass;
        ghost.linearDamping = rb.linearDamping;
        ghost.angularDamping = rb.angularDamping;
        ghost.useGravity = rb.useGravity;
        ghost.position = objectPos;
        ghost.linearVelocity = forceApplied / ghost.mass;
        ghost.isKinematic = false;

        trajectoryPoints.Clear();
        

        for (int i = 0; i < dotsNumber; i++)
        {
            float timeStep = Time.fixedDeltaTime;
            trajectoryPoints.Add(ghost.position);
            Physics.Simulate(timeStep);
        }

        Physics.simulationMode = SimulationMode.FixedUpdate;
        Destroy(ghostObj);

    }


    public void Show()
    {
        dotsParent.SetActive(true);
    }

    public void Hide()
    {
        dotsParent.SetActive(false);
    }

    public void SetSimulation(bool _isSimulating)
    {
        isSimulating = _isSimulating;
    }
}
