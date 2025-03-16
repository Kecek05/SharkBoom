using Sortify;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private Image player1Image;
    [SerializeField] private Image player2Image;
    [SerializeField] private Slider player1HealthSlider;
    [SerializeField] private Slider player2HealthSlider;


    private void Start()
    {
        PlayerHealth.OnPlayerTakeDamage += PlayerHealth_OnPlayerTakeDamage;
    }

    private void PlayerHealth_OnPlayerTakeDamage(object playerSender, PlayerHealth.OnPlayerTakeDamageArgs e)
    {
        if(e.playableState == PlayableState.Player1Playing)
        {
            player1HealthSlider.value = CalculateHealthPercentage(e.playerCurrentHealth, e.playerMaxHealth);
        }
        else if (e.playableState == PlayableState.Player2Playing)
        {
            player2HealthSlider.value = CalculateHealthPercentage(e.playerCurrentHealth, e.playerMaxHealth);
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
