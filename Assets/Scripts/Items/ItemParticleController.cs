using UnityEngine;

public class ItemParticleController : MonoBehaviour
{
    [Header("Triggers References")]
    [SerializeField] private BaseCollisionController baseCollisionController;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem despawnParticleSystem;

    private void Start()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
    }

    private void HandleItemCollidedWithPlayer(PlayerThrower playerThrower)
    {
        HandleItemCollidedWithPlayer();
    }

    private void HandleItemCollidedWithPlayer()
    {
        despawnParticleSystem.Play();
    }
    private void OnDestroy()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
