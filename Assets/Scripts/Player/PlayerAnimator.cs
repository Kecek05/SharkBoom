using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Player player;




    private readonly static int[] animations =
    {
        Animator.StringToHash("Idle"),
        Animator.StringToHash("Shoot"),
        Animator.StringToHash("Jump"),
        Animator.StringToHash("AimJump"),
        Animator.StringToHash("Aim"),
    };

    private Animations currentAnimation;
    private bool locked;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;
        }
    }

    private void PlayerStateMachine_OnStateChanged(IState newState)
    {

    }

    private void ChangeAnimation(string newAnimation, float crossFade = 0.2f)
    {
        if(currentAnimation != newAnimation)
        {
            currentAnimation = newAnimation;
            animator.CrossFade(newAnimation, crossFade);

        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
        }
    }
}

public enum Animations
{
    None,
    Idle,
    Shoot,
    Jump,
    AimJump,
    Aim,
}
