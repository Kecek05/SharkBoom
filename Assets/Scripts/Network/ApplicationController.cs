using Sortify;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ApplicationController : MonoBehaviour //Responsable of launching the game in the correct mode
{

    [BetterHeader("Singletons")]
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;
    [SerializeField] private NetworkServerProvider networkServerProvider;

    [BetterHeader("Settings", 12)]
    [SerializeField] private NetworkObject playerPrefab;

    [SerializeField] private bool isServerDebug = false;

    private ApplicationData appData;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        if(isServerDebug)
        {
            await LaunchInMode(true);
            return;
        }

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        Instantiate(networkServerProvider);

        if (isDedicatedServer)
        {
            //Dedicated Server Code
            Application.targetFrameRate = 60; //Fixed fps

            appData = new();

            ServerSingleton serverSingleton = Instantiate(serverPrefab);

            StartCoroutine(LoadGameSceneAsync(serverSingleton));

        } else
        {
            //Host and Client Code

            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost(playerPrefab);

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            clientSingleton.CreateClient();

            //Go to main menu
            Loader.Load(Loader.Scene.AuthBootstrap);

        }
    }

    private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
    {
        //Refactor change scene. Need to be this way because we were trying to change the scene and the Client load it before the server was ready.
        AsyncOperation asyncLoadLoadingScene = Loader.LoadDSAsync(Loader.Scene.Loading);

        while(!asyncLoadLoadingScene.isDone)
        {
            //Not finished to load the loading scene
            yield return null;
        }


        AsyncOperation asyncLoadGameScene = Loader.LoadDSAsync(Loader.Scene.GameNetCodeTest);

        while(!asyncLoadGameScene.isDone)
        {
            //Not finished to load the scene
            yield return null;
        }
        //Finished loading the scene

        Task createServerTask = serverSingleton.CreateServer(playerPrefab);
        yield return new WaitUntil(() => createServerTask.IsCompleted);

        Task startServerTask = serverSingleton.GameManager.StartGameServerAsync();
        yield return new WaitUntil(() => startServerTask.IsCompleted);  
    }
}
