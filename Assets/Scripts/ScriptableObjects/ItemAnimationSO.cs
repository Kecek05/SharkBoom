using UnityEngine;

[CreateAssetMenu(fileName = "ItemAnimationSO", menuName = "Scriptable Objects/ItemAnimationSO")]
public class ItemAnimationSO : ScriptableObject
{
    public AnimationData aimItemData;
    public AnimationData shootItemData;
}
