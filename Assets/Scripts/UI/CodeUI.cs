using System.Collections;
using TMPro;
using UnityEngine;

public class CodeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeTextUI;

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
                codeTextUI.text = HostSingleton.Instance.GameManager.JoinCode;

            yield return waitUpdate;
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        if (newValue == GameState.GameStarted)
        {
            StopCoroutine(updateCodeTextUICoroutine);

            codeTextUI.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        gameStateManager.CurrentGameState.OnValueChanged -= GameState_OnValueChanged;
    }
}
