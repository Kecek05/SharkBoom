using QFSW.QC;
using Sortify;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DebuggingTools : NetworkBehaviour
{

    public static DebuggingTools Instance;

    [BetterHeader("References")]
    public TextMeshProUGUI debugGameStateText;
    public TextMeshProUGUI debugPlayableStateText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        debugGameStateText.text = GameFlowManager.Instance.CurrentGameState.Value.ToString();
        debugPlayableStateText.text = GameFlowManager.Instance.CurrentPlayableState.Value.ToString();
    }

}
