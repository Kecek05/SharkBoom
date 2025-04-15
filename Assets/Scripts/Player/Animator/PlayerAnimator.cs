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
        Animator.StringToHash("Idle_L"),
        Animator.StringToHash("Idle_R"),
        Animator.StringToHash("ShootHarpoon_L"),
        Animator.StringToHash("ShootHarpoon_R"),
        Animator.StringToHash("Jump_L"),
        Animator.StringToHash("Jump_R"),
        Animator.StringToHash("AimJump_L"),
        Animator.StringToHash("AimJump_R"),
        Animator.StringToHash("AimHarpoon_L"),
        Animator.StringToHash("AimHarpoon_R"),
    };

    private AnimationData idleAnimationData = new AnimationData(Animations.Idle_L, Animations.Idle_R);
    private AnimationData shootAnimationData = new AnimationData(Animations.ShootHarpoon_L, Animations.ShootHarpoon_R);
    private AnimationData jumpAnimationData = new AnimationData(Animations.Jump_L, Animations.Jump_R);
    private AnimationData aimJumpAnimationData = new AnimationData(Animations.AimJump_L, Animations.AimJump_R, 0f);
    private AnimationData aimAnimationData = new AnimationData(Animations.AimHarpoon_L, Animations.AimHarpoon_R);


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
            Debug.Log($"Playing Shooting item - is right: {isRight}");
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
    Idle_L,
    Idle_R,
    ShootHarpoon_L,
    ShootHarpoon_R,
    Jump_L,
    Jump_R,
    AimJump_L,
    AimJump_R,
    AimHarpoon_L,
    AimHarpoon_R,
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