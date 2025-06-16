using UnityEngine;

public class ItemActivableCallbackVFXComponent : MonoBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private ParticleSystem particleSystemOnActivated;

    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;
        particleSystemOnActivated.Clear();
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        PlayVFX();
    }

    private void PlayVFX()
    {
        particleSystemOnActivated.Clear();
        particleSystemOnActivated.Play();
    }
    
    //
    // [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    // private void PlayVFXServerRpc()
    // {
    //     PlayVFXClientRpc();
    // }
    //
    // [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
    // private void PlayVFXClientRpc()
    // {
    //     particleSystemOnActivated.Clear();
    //     particleSystemOnActivated.Play();
    // }

    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
