using UnityEngine;

public class ItemSocket : MonoBehaviour
{
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private Transform trajectoryTransform;
    public ItemSO ItemSO => itemSO;

    public Transform TrajectoryTransform => trajectoryTransform;
}
