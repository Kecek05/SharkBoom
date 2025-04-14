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
            dotsList[i] = Instantiate(dotPrefab2D, dotsParent.transform).transform; // Create dots based on the number of dots variable
            dotsList[i].position = dotsParent.transform.position; // set the dots position to the parent position (in player)
            dotsList[i].gameObject.SetActive(false); // hide the dots, because we will show them when the distance is enough
        }
    }

    public void UpdateDots(Vector2 objectPos, Vector2 forceApplied, float maxForce, Rigidbody2D rb) 
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

    private void SimulateTrajectory(Vector2 objectPos, Vector2 forceApplied, Rigidbody2D rb)
    {
        if (!isSimulating) return;

        Physics2D.simulationMode = SimulationMode2D.Script;

        GameObject ghostObj = new GameObject("Ghost");
        Rigidbody2D ghost = ghostObj.AddComponent<Rigidbody2D>();

        ghost.mass = rb.mass;
        ghost.linearDamping = rb.linearDamping;
        ghost.angularDamping = rb.angularDamping;
        ghost.gravityScale = rb.gravityScale;
        ghost.position = objectPos;
        ghost.linearVelocity = forceApplied / ghost.mass;
        ghost.bodyType = RigidbodyType2D.Dynamic;

        trajectoryPoints.Clear();
        

        for (int i = 0; i < dotsNumber; i++)
        {
            float timeStep = Time.fixedDeltaTime;
            trajectoryPoints.Add(ghost.position);
            Physics2D.Simulate(timeStep);
        }

        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
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
