using Sortify;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject dotPrefab;

    [BetterHeader("Settings")]
    [Range(3, 50)]
    [SerializeField] private int dotsNumber;
    [RangeStep(0.01f, 1f, 0.01f)]
    [SerializeField] private float dotSpacing;

    private Vector3 dotPos;
    private float timeStamp; // Position of dots along the trajectory
    private Transform[] dotsList;
    private GameObject dotsParent;


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

    public void UpdateDots(Vector3 objectPos, Vector3 forceApplied)
    {
        timeStamp = dotSpacing; 
        for (int i = 0; i < dotsNumber; i++)
        {
            dotPos.x = objectPos.x + forceApplied.x * timeStamp; // Formula to calculate the position of the dots along the trajectory
            dotPos.y = objectPos.y + forceApplied.y * timeStamp + (0.5f * Physics.gravity.y * timeStamp * timeStamp);
            dotPos.z = objectPos.z + forceApplied.z * timeStamp; // we have to maintain the z position, for the dots to be in the same plane as the player

            dotsList[i].position = dotPos;
            timeStamp += dotSpacing; // increase the time stamp to move the dots further along the trajectory
        }
    }

    public void Show()
    {
        dotsParent.SetActive(true);
    }

    public void Hide()
    {
        dotsParent.SetActive(false);
    }
}
