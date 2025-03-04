using Sortify;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{

    [BetterHeader("Singletons")]
    //[SerializeField] private ClientSingleton clientPrefab;
    //[SerializeField] private HostSingleton hostPrefab;
    //[SerializeField] private ServerSingleton serverPrefab;

    [SerializeField] private NetworkObject playerPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if(isDedicatedServer)
        {
            //Dedicated Server Code
        } else
        {
            //Host and Client Code


        }
    }

}
