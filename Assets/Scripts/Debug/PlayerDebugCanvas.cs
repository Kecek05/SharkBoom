using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDebugCanvas : MonoBehaviour
{

    [BetterHeader("References")]
    public PlayerThrower playerThrower;
    public PlayerDetectFacingDirection playerDetectFacingDirection;
    public PlayerAnimator playerAnimator;
    public PlayerSpawnItemOnHand playerSpawnItemOnHand;

    [BetterHeader("Texts")]
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;
    public TextMeshProUGUI text5;
    public TextMeshProUGUI text6;
    public TextMeshProUGUI text7;
    public TextMeshProUGUI text8;
    private void Update()
    { 
        if(!playerThrower) return;

        if (playerThrower?.PlayerStateMachine != null)
        {
            text1.text = $"Player State: {playerThrower?.PlayerStateMachine?.CurrentState}";
        }

        if (playerThrower?.PlayerInventory)
        {
            text2.text = $"Can Interact Inv: {playerThrower.PlayerInventory?.CanInteractWithInventory}";
            text3.text = $"Inv ID: {playerThrower?.PlayerInventory?.SelectedItemID}";
        }

        if (playerThrower)
        {
            text4.text = $"Playable State: {playerThrower?.ThisPlayableState?.Value}";
        }

        if (playerDetectFacingDirection)
        {
            text5.text = $"Is Direction Right: {playerDetectFacingDirection?.IsDirectionRight}";
        }

        if (playerAnimator)
        {
            text6.text = $"Animator State: {playerAnimator?.CurrentAnimation}";
        }

        if (playerSpawnItemOnHand != null)
        {
            // if(playerSpawnItemOnHand.SelectedSocketTransform != null)
            //     text7.text = $"Item Socket Selected: {playerSpawnItemOnHand?.SelectedSocketTransform?.name}";
            
            text8.text = $"Item On Hand: {playerSpawnItemOnHand?.SpawnedItem?.name ?? "None"}";
        }

    }
}
