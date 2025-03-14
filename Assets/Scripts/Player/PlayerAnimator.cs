using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Player player;
    private string currentAnimation = "";

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
