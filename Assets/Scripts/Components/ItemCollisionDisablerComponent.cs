using UnityEngine;

public class ItemCollisionDisablerComponent : MonoBehaviour
{
    [SerializeField] private Collider2D[] itemColliders;
    public void DisableCollisions()
    {
        foreach (Collider2D itemCol in itemColliders)
        {
            itemCol.enabled = false;
        }
        Debug.Log("Item Coll OFF");
    }
}
