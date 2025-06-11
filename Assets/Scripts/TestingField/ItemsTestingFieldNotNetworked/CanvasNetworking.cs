using Unity.Netcode;
using UnityEngine;

public class CanvasNetworking : MonoBehaviour
{
    [SerializeField] private GameObject backgroundCanvas;
    
    public void StartClient()
    {   
        NetworkManager.Singleton.StartClient();
        Hide();
    }
    
    public void StartHost()
    {   
        NetworkManager.Singleton.StartHost();
        Hide();
    }

    private void Hide()
    {
        backgroundCanvas.SetActive(false);
    }
}
