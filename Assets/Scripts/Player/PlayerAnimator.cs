using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Animator animator;
    private bool isRight; //Rotation that the player is looking
    private bool isDefaultRight; // Rotation of the player thrower obj in idle. - TODO: Make it look to the direction of the other player

    private readonly static int[] animations =
    {
        Animator.StringToHash("IdleL"),
        Animator.StringToHash("IdleR"),
        Animator.StringToHash("ShootL"),
        Animator.StringToHash("ShootR"),
        Animator.StringToHash("JumpL"),
        Animator.StringToHash("JumpR"),
        Animator.StringToHash("AimJumpL"),
        Animator.StringToHash("AimJumpR"),
        Animator.StringToHash("AimL"),
        Animator.StringToHash("AimR"),
    };

    private AnimationData idleAnimationData = new AnimationData(Animations.IdleL, Animations.IdleR);
    private AnimationData shootAnimationData = new AnimationData(Animations.ShootL, Animations.ShootR);
    private AnimationData jumpAnimationData = new AnimationData(Animations.JumpL, Animations.JumpR);
    private AnimationData aimJumpAnimationData = new AnimationData(Animations.AimJumpL, Animations.AimJumpR, 0f);
    private AnimationData aimAnimationData = new AnimationData(Animations.AimL, Animations.AimR);


    private Animations currentAnimation;
    private AnimationData currentAnimationData;

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        if(!IsOwner) return; //only owner

        if (newState == PlayerState.IdleMyTurn || newState == PlayerState.IdleEnemyTurn || newState == PlayerState.MyTurnEnded)
        {
            PlayAnimationData(idleAnimationData);
        }
        else if (newState == PlayerState.DraggingItem)
        {
            PlayAnimationData(aimAnimationData);
        }
        else if (newState == PlayerState.DraggingJump)
        {
            PlayAnimationData(aimJumpAnimationData);
        }
        else if (newState == PlayerState.DragReleaseItem)
        {
            PlayAnimationData(shootAnimationData);
        }
        else if (newState == PlayerState.DragReleaseJump)
        {
            PlayAnimationData(jumpAnimationData);
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
        PlayAnimationData(currentAnimationData);
    }

    private void PlayAnimationData(AnimationData animationData)
    {
        if (animationData.animationL == Animations.None && animationData.animationR == Animations.None) return; //none

        //if (currentAnimation == animationData.animationL || currentAnimation == animationData.animationR) return; //already playing this animation

        currentAnimationData = animationData;

        if (isRight)
        {
            if(currentAnimation == animationData.animationR) return; //already playing this animation

            currentAnimation = animationData.animationR;
            animator.CrossFade(animations[(int)animationData.animationR], animationData.crossFade);
        }
        else
        {
            if(currentAnimation == animationData.animationL) return; //already playing this animation

            currentAnimation = animationData.animationL;
            animator.CrossFade(animations[(int)animationData.animationL], animationData.crossFade);
        }
    }
}

public enum Animations
{
    IdleL,
    IdleR,
    ShootL,
    ShootR,
    JumpL,
    JumpR,
    AimJumpL,
    AimJumpR,
    AimL,
    AimR,
    None, //at the bottom!
}

public struct AnimationData
{
    public Animations animationL;
    public Animations animationR;
    public float crossFade;
    public AnimationData(Animations animationL = Animations.None, Animations animationR = Animations.None, float crossFade = 0.2f)
    {
        this.animationL = animationL;
        this.animationR = animationR;
        this.crossFade = crossFade;
    }
}