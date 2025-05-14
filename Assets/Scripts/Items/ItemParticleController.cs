using UnityEngine;

public class ItemParticleController : MonoBehaviour
{
    [Header("Triggers")]
    [SerializeField] private BaseCollisionController itemCollisionController;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem despawnParticleSystem;

    private void Start()
    {
        itemCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
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
        itemCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
