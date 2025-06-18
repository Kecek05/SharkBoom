using TMPro;
using UnityEngine;

public class ShowPlayerID : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerID;
#if UNITY_ANDROID
    private void Start()
    {
        playerID.text = $"ID: {AuthenticationWrapper.GetPlayerID()}";
    } 
#endif
}
