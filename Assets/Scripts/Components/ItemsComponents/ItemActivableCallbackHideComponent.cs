using UnityEngine;

public class ItemActivableCallbackHideComponent : MonoBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private GameObject meshToHide;
    [SerializeField] private HideMeshOnCollisionComponent hideMeshOnCollisionComponent;
    
    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        hideMeshOnCollisionComponent.HideMesh();
    }

    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
