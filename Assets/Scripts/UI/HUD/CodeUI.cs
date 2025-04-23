using System.Collections;
using TMPro;
using UnityEngine;

public class CodeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeTextUI;
    [SerializeField] private GameObject codeUIBackground;

    private BaseGameStateManager gameStateManager;

    private WaitForSeconds waitUpdate = new WaitForSeconds(1f);

    private Coroutine updateCodeTextUICoroutine;

    private void Start()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();

        gameStateManager.CurrentGameState.OnValueChanged += GameState_OnValueChanged;

        updateCodeTextUICoroutine = StartCoroutine(UpdateCodeTextUI());
    }


    private IEnumerator UpdateCodeTextUI()
    {
        while (true)
        {
            if (HostSingleton.Instance != null)
            {
                if(HostSingleton.Instance.GameManager.JoinCode == string.Empty || HostSingleton.Instance.GameManager.JoinCode == null)
                {
                    //No lobby game
                    codeUIBackground.gameObject.SetActive(false);
                } else
                {
                    codeTextUI.text = HostSingleton.Instance.GameManager.JoinCode;
                    codeUIBackground.gameObject.SetActive(true);
                }

            }
                

            yield return waitUpdate;
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        if (newValue == GameState.ShowingPlayersInfo)
        {
            StopCoroutine(updateCodeTextUICoroutine);

            codeUIBackground.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        gameStateManager.CurrentGameState.OnValueChanged -= GameState_OnValueChanged;
    }
}
