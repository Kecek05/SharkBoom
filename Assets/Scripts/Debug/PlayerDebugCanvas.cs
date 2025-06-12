using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDebugCanvas : MonoBehaviour
{

    [BetterHeader("References")]
    public PlayerThrower playerThrower;

    [BetterHeader("Texts")]
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;
    public TextMeshProUGUI text5;
    public TextMeshProUGUI text6;
    private void Update()
    {
       text1.text = $"Player State: {playerThrower.PlayerStateMachine.CurrentState}";
       text2.text = $"Can Interact Inv: {playerThrower.PlayerInventory.CanInteractWithInventory}";
       text3.text = $"Inv ID: {playerThrower.PlayerInventory.SelectedItemID}";
    }
}
