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

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        if(!IsOwner) return; //only owner

        if (newState == PlayerState.IdleMyTurn || newState == PlayerState.IdleEnemyTurn || newState == PlayerState.MyTurnEnded)
        {
            PlayAnimation(Animations.Idle);
        } 
        else if (newState == PlayerState.DraggingItem)
        {
            PlayAnimation(Animations.Aim);
        }
        else if (newState == PlayerState.DraggingJump)
        {
            PlayAnimation(Animations.AimJump, 0);
        }
        else if (newState == PlayerState.DragReleaseItem)
        {
            PlayAnimation(Animations.Shoot);
        }
        else if (newState == PlayerState.DragReleaseJump)
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