using Sortify;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageableSO", menuName = "Scriptable Objects/DamageableSO")]
public class DamageableSO : ScriptableObject
{
    public float damage;

    [BetterHeader("Item Damage Multiplier", 10)]
    public float headMultiplier = 1f;
    public float bodyMultiplier = 1f;
    public float footMultiplier = 1f;
}
