using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerThrower player;

    private readonly static int[] animations =
    {
        Animator.StringToHash("Idle"),
        Animator.StringToHash("Shoot"),
        Animator.StringToHash("Jump"),
        Animator.StringToHash("AimJump"),
        Animator.StringToHash("Aim"),
    };

    private Animations currentAnimation;


    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;
        }
    }

    private void PlayerStateMachine_OnStateChanged(IState newState)
    {
        if(newState == player.PlayerStateMachine.idleMyTurnState || newState == player.PlayerStateMachine.idleEnemyTurnState || newState == player.PlayerStateMachine.myTurnEndedState)
        {
            PlayAnimation(Animations.Idle);
        } 
        else if (newState == player.PlayerStateMachine.draggingItem)
        {
            PlayAnimation(Animations.Aim);
        }
        else if (newState == player.PlayerStateMachine.draggingJump)
        {
            PlayAnimation(Animations.AimJump, 0);
        }
        else if (newState == player.PlayerStateMachine.dragReleaseItem)
        {
            PlayAnimation(Animations.Shoot);
        }
        else if (newState == player.PlayerStateMachine.dragReleaseJump)
        {
            PlayAnimation(Animations.Jump);
        }
    }

    private void PlayAnimation(Animations newAnimation, float crossFade = 0.2f)
    {
        if(newAnimation == Animations.None) return; //none

        if (currentAnimation == newAnimation) return; //already playing this animation

        currentAnimation = newAnimation;

        animator.CrossFade(animations[(int)currentAnimation], crossFade);

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
    Idle,
    Shoot,
    Jump,
    AimJump,
    Aim,
    None, //at the bottom!
}