using TMPro;
using UnityEngine;

public class ShowPlayerID : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerID;

    private void Start()
    {
        playerID.text = $"ID: {AuthenticationWrapper.GetPlayerID()}";
    }
}
