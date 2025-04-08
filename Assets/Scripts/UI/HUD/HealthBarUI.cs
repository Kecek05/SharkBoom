using DG.Tweening;
using Sortify;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private Image player1Image;
    [SerializeField] private Image player2Image;
    [SerializeField] private Slider player1HealthSlider;
    [SerializeField] private Slider player2HealthSlider;
    [SerializeField] private float tweenDuration = 0.3f;

    private void Start()
    {
        PlayerHealth.OnPlayerTakeDamage += PlayerHealth_OnPlayerTakeDamage;

        SyncAtStart();
    }

    private void SyncAtStart()
    {
        //Needed to resync reconnected player

        GameObject player1Obj = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(PlayableState.Player1Playing);

        GameObject player2Obj = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(PlayableState.Player2Playing);

        if(player1Obj != null)
        {
            PlayerHealth player1Health = player1Obj.GetComponent<PlayerHealth>();

            PlayerHealth.OnPlayerTakeDamageArgs onPlayerTakeDamageArgsPlayer1 = new PlayerHealth.OnPlayerTakeDamageArgs
            {
                playableState = PlayableState.Player1Playing,
                playerCurrentHealth = player1Health.CurrentHealth.Value,
                playerMaxHealth = player1Health.MaxHealth
            };
            PlayerHealth_OnPlayerTakeDamage(null, onPlayerTakeDamageArgsPlayer1);
        }

        if(player2Obj != null)
        {
            PlayerHealth player2Health = player2Obj.GetComponent<PlayerHealth>();

            PlayerHealth.OnPlayerTakeDamageArgs onPlayerTakeDamageArgsPlayer2 = new PlayerHealth.OnPlayerTakeDamageArgs
            {
                playableState = PlayableState.Player2Playing,
                playerCurrentHealth = player2Health.CurrentHealth.Value,
                playerMaxHealth = player2Health.MaxHealth
            };


            PlayerHealth_OnPlayerTakeDamage(null, onPlayerTakeDamageArgsPlayer2);
        }
    }

    private void PlayerHealth_OnPlayerTakeDamage(object playerSender, PlayerHealth.OnPlayerTakeDamageArgs e)
    {
        if(e.playableState == PlayableState.Player1Playing)
        {
            DOTween.To(() => player1HealthSlider.value, x => player1HealthSlider.value = x, CalculateHealthPercentage(e.playerCurrentHealth, e.playerMaxHealth), tweenDuration);
        }
        else if (e.playableState == PlayableState.Player2Playing)
        {
            DOTween.To(() => player2HealthSlider.value, x => player2HealthSlider.value = x, CalculateHealthPercentage(e.playerCurrentHealth, e.playerMaxHealth), tweenDuration);
        }


    }

    private float CalculateHealthPercentage(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }



    private void OnDestroy()
    {
        PlayerHealth.OnPlayerTakeDamage -= PlayerHealth_OnPlayerTakeDamage;
    }
}
