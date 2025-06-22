using UnityEngine;

public class ItemActivableCallbackVFXComponent : MonoBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private ParticleSystem[] particleSystemOnActivated;

    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;

        ParticleSystemClear();
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        PlayVFX();
    }

    private void PlayVFX()
    {
        ParticleSystemClear();
        ParticleSystemPlay();
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

    private void ParticleSystemClear()
    {
        foreach (ParticleSystem particleSystem in particleSystemOnActivated)
        {
            particleSystem.Clear();
        }
    }

    private void ParticleSystemPlay()
    {
        foreach (ParticleSystem particleSystem in particleSystemOnActivated)
        {
            particleSystem.Play();
        }
    }
        
    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
