using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [SerializeField] private int dotsNumber;
    [SerializeField] private GameObject dotsParent;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private float dotSpacing;

    private Vector3 dotPos;
    private float timeStamp;
    private Transform[] dotsList;

    private void Start()
    {
        PrepareDots();
        Hide();
    }

    private void PrepareDots()
    {
        dotsList = new Transform[dotsNumber];
        for (int i = 0; i < dotsNumber; i++)
        {
            dotsList[i] = Instantiate(dotPrefab, dotsParent.transform).transform;
            dotsList[i].position = dotsParent.transform.position; // Garante que os pontos iniciem na posição correta
        }
    }

    public void UpdateDots(Vector3 objectPos, Vector3 forceApplied)
    {
        timeStamp = dotSpacing;
        for (int i = 0; i < dotsNumber; i++)
        {
            dotPos.x = objectPos.x + forceApplied.x * timeStamp;
            dotPos.y = objectPos.y + forceApplied.y * timeStamp + (0.5f * Physics.gravity.y * timeStamp * timeStamp);
            dotPos.z = objectPos.z + forceApplied.z * timeStamp; // Para considerar 3D

            dotsList[i].position = dotPos;
            timeStamp += dotSpacing;
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
