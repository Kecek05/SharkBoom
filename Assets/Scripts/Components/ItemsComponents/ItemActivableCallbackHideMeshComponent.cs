using UnityEngine;

public class ItemActivableCallbackHideMeshComponent : MonoBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private GameObject meshToHide;

    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        meshToHide.SetActive(false);
    }

    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
