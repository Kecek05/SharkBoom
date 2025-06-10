using UnityEngine;

[CreateAssetMenu(fileName = "KnockbackSO", menuName = "Scriptable Objects/KnockbackSO")]
public class KnockbackSO : ScriptableObject
{
    /// <summary>
    ///  The force applied to the object when knocked back
    /// </summary>
    public float knockbackStrength = 200f;
}
