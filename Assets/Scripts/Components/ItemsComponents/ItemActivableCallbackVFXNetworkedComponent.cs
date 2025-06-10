using Unity.Netcode;
using UnityEngine;

public class ItemActivableCallbackVFXNetworkedComponent : NetworkBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private ParticleSystem particleSystemOnActivated;

    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        PlayVFXServerRpc();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void PlayVFXServerRpc()
    {
        PlayVFXClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
    private void PlayVFXClientRpc()
    {
        particleSystemOnActivated.Play();
    }

    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
