using Unity.Netcode;
using UnityEngine;

public class ItemActivableCallbackHideMeshNetworkedComponent : NetworkBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private GameObject meshToHide;

    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;
        meshToHide.SetActive(true);
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        HideMeshServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void HideMeshServerRpc()
    {
        HideMeshClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HideMeshClientRpc()
    {
        meshToHide.SetActive(false);
    }

    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
