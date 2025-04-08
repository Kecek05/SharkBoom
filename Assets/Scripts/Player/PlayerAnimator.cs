using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Animator animator;
    private bool isRight;

    private readonly static int[] animations =
    {
        Animator.StringToHash("Idle"),
        Animator.StringToHash("ShootL"),
        Animator.StringToHash("ShootR"),
        Animator.StringToHash("JumpL"),
        Animator.StringToHash("JumpR"),
        Animator.StringToHash("AimJumpL"),
        Animator.StringToHash("AimJumpR"),
        Animator.StringToHash("AimL"),
        Animator.StringToHash("AimR"),
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
            if (isRight)
            {
                PlayAnimation(Animations.AimR);
            }
            else
            {
                PlayAnimation(Animations.AimL);
            }
        }
        else if (newState == PlayerState.DraggingJump)
        {
            if(isRight)
            {
                PlayAnimation(Animations.AimJumpR, 0);
            } else
            {
                PlayAnimation(Animations.AimJumpL, 0);
            }
        }
        else if (newState == PlayerState.DragReleaseItem)
        {
            if (isRight)
            {
                PlayAnimation(Animations.ShootR);
            }
            else
            {
                PlayAnimation(Animations.ShootL);
            }
        }
        else if (newState == PlayerState.DragReleaseJump)
        {
            if (isRight)
            {
                PlayAnimation(Animations.JumpR);
            }
            else
            {
                PlayAnimation(Animations.JumpL);
            }
        }
    }

    public void HandleOnRotationChanged(bool isRight)
    {
        if (!IsOwner) return;

        this.isRight = isRight;

        RotationChanged();
    }

    private void RotationChanged()
    {
        PlayAnimation(currentAnimation);
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
    ShootL,
    ShootR,
    JumpL,
    JumpR,
    AimJump,
    AimJumpL,
    AimJumpR,
    AimL,
    AimR,
    None, //at the bottom!
}