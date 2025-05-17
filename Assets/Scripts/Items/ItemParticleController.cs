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
        Debug.Log("ItemParticleController Start");
        hideMeshOnCollisionComponent.OnMeshHidden += HideMeshOnCollisionComponent_OnMeshHidden;
        PlayParticleSystem(spawnParticleSystem);
    }

    private void HideMeshOnCollisionComponent_OnMeshHidden()
    {
        PlayParticleSystem(despawnParticleSystem);
    }

    private void PlayParticleSystem(ParticleSystem particleSystem)
    {
        Debug.Log($"PlayParticleSystem: {particleSystem}");
        particleSystem.Play();
    }

    private void OnDestroy()
    {
        hideMeshOnCollisionComponent.OnMeshHidden -= HideMeshOnCollisionComponent_OnMeshHidden;
    }
}
