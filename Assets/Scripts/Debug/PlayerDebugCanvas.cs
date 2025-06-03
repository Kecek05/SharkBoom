using Sortify;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerDebugCanvas : NetworkBehaviour
{

    [BetterHeader("References")]
    public PlayerGetUp playerGetUp;
    public TextMeshProUGUI isFallenTxt;
    public TextMeshProUGUI isOwnerTxt;
    public TextMeshProUGUI calculatedPosTxt;
    public TextMeshProUGUI originalRootZTxt;
    public TextMeshProUGUI verticalOffsetTxt;
    public TextMeshProUGUI originalHipsRotTxt;
    public TextMeshProUGUI originalRootRotTxt;

    private void Update()
    {
        isOwnerTxt.text = $"Is Owner: {IsOwner}";
        isFallenTxt.text = $"Is Fallen: {playerGetUp.IsFallen}";
        calculatedPosTxt.text = $"Calculated Pos: {playerGetUp.FinalPosition} - FinalRotation: {playerGetUp.recievedFinalRotation} - OriginalHipsRotation: {playerGetUp.recievedOriginalHipsRotation}";
        originalHipsRotTxt.text = $"Original Hips Rotation: {playerGetUp.OriginalHipsRotation}";
        originalRootZTxt.text = $"Original Root Z: {playerGetUp.OriginalRootZ}";
        verticalOffsetTxt.text = $"Vertical Offset: {playerGetUp.VerticalOffset}";
    }
}
