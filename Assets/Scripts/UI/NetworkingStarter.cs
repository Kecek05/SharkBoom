using Sortify;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkingStarter : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject background;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button hostBtn;

    private void Awake()
    {
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            background.SetActive(false);
        });

        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            background.SetActive(false);
        });

    }
}
