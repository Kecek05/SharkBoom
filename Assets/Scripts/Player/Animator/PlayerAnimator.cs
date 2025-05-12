using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    public event Action OnCrossfadeFinished;

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
    private AnimationData aimJumpAnimationData = new AnimationData(Animations.AimJump_L, Animations.AimJump_R);


    private Animations currentAnimation;
    private AnimationData currentAnimationData;

    private AnimationData selectedAimAnimation;
    private AnimationData selectedShootAnimation;

    private Coroutine crossFadeCoroutine;

    public void HandleOnItemSelectedSO(ItemSO itemSelectedSO)
    {
        selectedAimAnimation = itemSelectedSO.itemAnimationSO.aimItemData;

        selectedShootAnimation = itemSelectedSO.itemAnimationSO.shootItemData;
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
        Debug.Log("Rotation Changed on Animation");
    }

    private void PlayAnimationData(AnimationData animationData)
    {
        Debug.Log($"$Animation Data - Animation L: {animationData.animationL} - Animation R: {animationData.animationR}");

        if (animationData.animationL == Animations.None && animationData.animationR == Animations.None) return; //none

        //if (currentAnimation == animationData.animationL || currentAnimation == animationData.animationR) return; //already playing this animation

        if(animationData.Equals(currentAnimationData))
        {
            //Already playing this animation

            if(isRight)
            {
                //looking to the right

                if(currentAnimation == animationData.animationR) return; //already playing the right animation

                currentAnimation = animationData.animationR;
                DoCrossFade(animations[(int)animationData.animationR], animationData.crossFadeBetweenSides);
            } else
            {
                //looking to the left

                if (currentAnimation == animationData.animationL) return; //already playing the left animation

                currentAnimation = animationData.animationL;
                DoCrossFade(animations[(int)animationData.animationL], animationData.crossFadeBetweenSides);
            }
        } else
        {
            // not the same animation

            if (isRight)
            {
                //looking to the right

                currentAnimation = animationData.animationR;
                DoCrossFade(animations[(int)animationData.animationR], animationData.crossFade);
            }
            else
            {
                //looking to the left

                currentAnimation = animationData.animationL;
                DoCrossFade(animations[(int)animationData.animationL], animationData.crossFade);
            }
        }
        currentAnimationData = animationData;
    }

    private void DoCrossFade(int stateHashName, float fadeTime)
    {
        if(crossFadeCoroutine != null)
        {
            StopCoroutine(crossFadeCoroutine);
            crossFadeCoroutine = null;
        }

        animator.CrossFade(stateHashName, fadeTime);

        crossFadeCoroutine = StartCoroutine(CrossFadeCallback());
    }

    private IEnumerator CrossFadeCallback()
    {
        // Wait until the animator is in transition
        while (!animator.IsInTransition(0))
        {
            yield return null;
        }

        // Wait until the transition has ended
        while (animator.IsInTransition(0))
        {
            yield return null;
        }

        // Transition has ended
        Debug.Log("Crossfade Finished!");
        OnCrossfadeFinished?.Invoke();
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
public struct AnimationData : IEquatable<AnimationData>
{
    [BetterHeader("Animations", 12)]
    public Animations animationL;
    public Animations animationR;
    [Space(7)]

    [BetterHeader("Crossfades", 12)]
    /// <summary>
    /// Crossfade time in seconds in Fade In
    /// </summary>
    [Tooltip("Crossfade time in seconds in Fade In")]
    public float crossFade;

    /// <summary>
    /// Crossfade time in seconds when changing between left and right
    /// </summary>
    [Tooltip("Crossfade time in seconds when changing between left and right")]
    public float crossFadeBetweenSides;

    public AnimationData(Animations animationL = Animations.None, Animations animationR = Animations.None, float crossFade = 0.3f, float crossFadeBetweenSides = 0f)
    {
        this.animationL = animationL;
        this.animationR = animationR;
        this.crossFade = crossFade;
        this.crossFadeBetweenSides = crossFadeBetweenSides;
    }

    public bool Equals(AnimationData other)
    {
        return
            animationL == other.animationL &&
            animationR == other.animationR &&
            crossFade == other.crossFade &&
            crossFadeBetweenSides == other.crossFadeBetweenSides;
    }
}