using System;
using UnityEngine;

public class ItemParticleController : MonoBehaviour
{
    [Header("Triggers References")]
    [SerializeField] private HideMeshOnCollisionComponent hideMeshOnCollisionComponent;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem despawnParticleSystem;
    [SerializeField] private ParticleSystem spawnParticleSystem;

    private void Start()
    {
        hideMeshOnCollisionComponent.OnMeshHidden += HideMeshOnCollisionComponent_OnMeshHidden;
        BaseItemThrowable.OnItemInitialized += BaseItemThrowable_OnItemInitialized;
    }

    private void BaseItemThrowable_OnItemInitialized()
    {
        PlayParticleSystem(spawnParticleSystem);
    }

    private void HideMeshOnCollisionComponent_OnMeshHidden()
    {
        PlayParticleSystem(despawnParticleSystem);
    }

    private void PlayParticleSystem(ParticleSystem particleSystem)
    {
        particleSystem.Play();
    }

    private void OnDestroy()
    {
        hideMeshOnCollisionComponent.OnMeshHidden -= HideMeshOnCollisionComponent_OnMeshHidden;
        BaseItemThrowable.OnItemInitialized -= BaseItemThrowable_OnItemInitialized;
    }
}
