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
