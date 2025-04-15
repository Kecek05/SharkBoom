using Sortify;
using System;
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
        Animator.StringToHash("AimJump_L"),
        Animator.StringToHash("AimJump_R"),
        Animator.StringToHash("AimAnchor_L"),
        Animator.StringToHash("AimAnchor_R"),
        Animator.StringToHash("AimBagOfMoney_L"),
        Animator.StringToHash("AimBagOfMoney_R"),
        Animator.StringToHash("AimBanana_L"),
        Animator.StringToHash("AimBanana_R"),
        Animator.StringToHash("AimBarrel_L"),
        Animator.StringToHash("AimBarrel_R"),
        Animator.StringToHash("AimBomb_L"),
        Animator.StringToHash("AimBomb_R"),
        Animator.StringToHash("AimCoconut_L"),
        Animator.StringToHash("AimCoconut_R"),
        Animator.StringToHash("AimHarpoon_L"),
        Animator.StringToHash("AimHarpoon_R"),
        Animator.StringToHash("AimMolotov_L"),
        Animator.StringToHash("AimMolotov_R"),
        Animator.StringToHash("AimSeaStar_L"),
        Animator.StringToHash("AimSeaStar_R"),
        Animator.StringToHash("AimSword_L"),
        Animator.StringToHash("AimSword_R"),
        Animator.StringToHash("Jump_L"),
        Animator.StringToHash("Jump_R"),
        Animator.StringToHash("ShootAnchor_L"),
        Animator.StringToHash("ShootAnchor_R"),
        Animator.StringToHash("ShootBagOfMoney_L"),
        Animator.StringToHash("ShootBagOfMoney_R"),
        Animator.StringToHash("ShootBanana_L"),
        Animator.StringToHash("ShootBanana_R"),
        Animator.StringToHash("ShootBarrel_L"),
        Animator.StringToHash("ShootBarrel_R"),
        Animator.StringToHash("ShootBomb_L"),
        Animator.StringToHash("ShootBomb_R"),
        Animator.StringToHash("ShootCoconut_L"),
        Animator.StringToHash("ShootCoconut_R"),
        Animator.StringToHash("ShootHarpoon_L"),
        Animator.StringToHash("ShootHarpoon_R"),
        Animator.StringToHash("ShootMolotov_L"),
        Animator.StringToHash("ShootMolotov_R"),
        Animator.StringToHash("ShootSeaStar_L"),
        Animator.StringToHash("ShootSeaStar_R"),
        Animator.StringToHash("ShootSword_L"),
        Animator.StringToHash("ShootSword_R"),
    };

    private AnimationData idleAnimationData = new AnimationData(Animations.Idle_L, Animations.Idle_R);
    private AnimationData jumpAnimationData = new AnimationData(Animations.Jump_L, Animations.Jump_R);
    private AnimationData aimJumpAnimationData = new AnimationData(Animations.AimJump_L, Animations.AimJump_R, 0f);


    private Animations currentAnimation;
    private AnimationData currentAnimationData;

    private AnimationData selectedAimAnimation;
    private AnimationData selectedShootAnimation;

    public void HandleOnItemSelectedSO(ItemSO itemSelectedSO)
    {
        selectedAimAnimation = itemSelectedSO.aimItemData;

        selectedShootAnimation = itemSelectedSO.shootItemData;
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        if(!IsOwner) return; //only owner

        if (newState == PlayerState.IdleMyTurn || newState == PlayerState.IdleEnemyTurn || newState == PlayerState.MyTurnEnded)
        {
            PlayAnimationData(idleAnimationData);
        }
        else if (newState == PlayerState.DraggingItem)
        {
            PlayAnimationData(selectedAimAnimation);
        }
        else if (newState == PlayerState.DraggingJump)
        {
            PlayAnimationData(aimJumpAnimationData);
        }
        else if (newState == PlayerState.DragReleaseItem)
        {
            PlayAnimationData(selectedShootAnimation);
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
    AimJump_L,
    AimJump_R,
    AimAnchor_L,
    AimAnchor_R,
    AimBagOfMoney_L,
    AimBagOfMoney_R,
    AimBanana_L,
    AimBanana_R,
    AimBarrel_L,
    AimBarrel_R,
    AimBomb_L,
    AimBomb_R,
    AimCoconut_L,
    AimCoconut_R,
    AimHarpoon_L,
    AimHarpoon_R,
    AimMolotov_L,
    AimMolotov_R,
    AimSeaStar_L,
    AimSeaStar_R,
    AimSword_L,
    AimSword_R,
    Jump_L,
    Jump_R,
    ShootAnchor_L,
    ShootAnchor_R,
    ShootBagOfMoney_L,
    ShootBagOfMoney_R,
    ShootBanana_L,
    ShootBanana_R,
    ShootBarrel_L,
    ShootBarrel_R,
    ShootBomb_L,
    ShootBomb_R,
    ShootCoconut_L,
    ShootCoconut_R,
    ShootHarpoon_L,
    ShootHarpoon_R,
    ShootMolotov_L,
    ShootMolotov_R,
    ShootSeaStar_L,
    ShootSeaStar_R,
    ShootSword_L,
    ShootSword_R,
    None, //at the bottom!
}

[Serializable]
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