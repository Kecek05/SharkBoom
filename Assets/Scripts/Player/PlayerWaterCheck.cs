using System.Collections;
using UnityEngine;

public class PlayerWaterCheck : MonoBehaviour
{
    [SerializeField] private GameObject playerGfx;
    [SerializeField] private PlayerBodyPart damageable;
    [SerializeField] private DamageableSO damageableSO;

    private const float waterLevel = -5f;
    private bool hasTakenDamage = false;
    private WaitForSeconds checkTime = new WaitForSeconds(0.5f);

    private void Start()
    {
        StartCoroutine(CheckWaterLevel());
    }

    private IEnumerator CheckWaterLevel()
    {
        while(true)
        {
            if (hasTakenDamage) break;
            
            if (playerGfx.transform.position.y < waterLevel)
            {
                damageable.TakeDamage(damageableSO);
                hasTakenDamage = true;
            }

            Debug.Log($"Player position: {playerGfx.transform.position.y}, Water level: {waterLevel}");
            yield return checkTime;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
