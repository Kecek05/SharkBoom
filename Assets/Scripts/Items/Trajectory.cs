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
    [RangeStep(0.01f, 1f, 0.01f)]
    [SerializeField] private float dotSpacing;
    [SerializeField] private float forceChangeThreshold = 0.1f;

    private Transform[] dotsList;
    private GameObject dotsParent;

    private List<Vector2> trajectoryPoints = new List<Vector2>();
    private bool isSimulating;

    
    
    private Vector2 lastForceApplied = Vector2.zero;
    private float currentForce;
    private float previousForce;
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

    public void UpdateDots(Vector2 objectPos, Vector2 forceApplied, float maxForce, Rigidbody rb) 
    {
        trajectoryPoints.Add(objectPos);
        SimulateTrajectory(objectPos, forceApplied, rb);

        currentForce = forceApplied.magnitude;
        previousForce = lastForceApplied.magnitude;
        diff = currentForce - previousForce;

        if (Mathf.Abs(diff) >= forceChangeThreshold)
        {
            int targetActiveDots = Mathf.Clamp(Mathf.RoundToInt((currentForce / maxForce) * dotsNumber), 0, dotsNumber);

            for (int i = 0; i < dotsNumber; i++)
            {
                dotsList[i].gameObject.SetActive(i < targetActiveDots);
            }

            lastForceApplied = forceApplied;
        }

        for (int i = 0; i < dotsNumber && i < trajectoryPoints.Count; i++)
        {
            dotsList[i].position = trajectoryPoints[i];
        }
    }

    private void SimulateTrajectory(Vector2 objectPos, Vector2 forceApplied, Rigidbody rb)
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
