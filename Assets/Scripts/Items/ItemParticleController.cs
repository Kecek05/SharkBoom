using System;
using UnityEngine;

public class ItemParticleController : MonoBehaviour
{
    [Header("Triggers References")]
    [SerializeField] private HideMeshOnCollisionComponent hideMeshOnCollisionComponent;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem despawnParticleSystem;
    [SerializeField] private ParticleSystem spawnParticleSystem;

    private void OnEnable()
    {
        hideMeshOnCollisionComponent.OnMeshHidden += HideMeshOnCollisionComponent_OnMeshHidden;
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

    private void OnDisable()
    {
        hideMeshOnCollisionComponent.OnMeshHidden -= HideMeshOnCollisionComponent_OnMeshHidden;
    }
}
