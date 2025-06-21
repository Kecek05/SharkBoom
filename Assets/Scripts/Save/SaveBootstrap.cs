using System.Threading.Tasks;
using QFSW.QC;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveBootstrap : MonoBehaviour
{
    private async void Start()
    {
       await UpdateSave();

       await CheckInMatch();
    }
    
    private async Task UpdateSave()
    {
        //Get save before in game
        ClientSingleton.Instance.GameManager.UserData.SetUserPearls(await Save.LoadPlayerPearls(ClientSingleton.Instance.GameManager.UserData.userAuthId));
    }

    private async Task CheckInMatch()
    {
        //Check if is in match
        if (await Reconnect.GetIsInMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId))
        {
            //Was in match, first check if the server is online

            //Is in match, reconnect
            string ipMatch = await Reconnect.GetIpMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId);
            int portMatch = await Reconnect.GetPortMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId);

            Debug.Log($"Match in progress, rejoin it...");
            ClientSingleton.Instance.GameManager.StartMatchmakingClient(ipMatch, portMatch);
            
            // if (await ServerConnectionTester.CheckIsOnline())
            // {
            //     Debug.Log($"Match in progress, rejoin it...");
            //     ClientSingleton.Instance.GameManager.StartMatchmakingClient(ipMatch, portMatch);
            //     
            //     if (await Reconnect.CanRejoinInMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId))
            //     {
            //         //Match in progress, join it
            //         Debug.Log($"Match in progress, rejoin it...");
            //         ClientSingleton.Instance.GameManager.StartMatchmakingClient(ipMatch, portMatch);
            //     }
            //     else
            //     {
            //         //Match is over
            //         Debug.Log($"Match is over, going to menu.");
            //         Reconnect.SetIsInMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId, false); //Not in match anymore
            //         Loader.LoadNoLoadingScreen(Loader.Scene.NameBootstrap);
            //     }
            // }
            // else
            // {
            //     Debug.Log($"Client is Offline!");
            //     ClientSingleton.Instance.GameManager.StartMatchmakingClient(ipMatch, portMatch);
            // }
        } else
        {
            //Not in match, go to Name Bootstrap
            Loader.LoadNoLoadingScreen(Loader.Scene.NameBootstrap);
        }
    }
}

public static class DebugSave
{
    
    [Command("Scene")]
    public static void PrintScene()
    {
        Debug.Log(SceneManager.GetActiveScene().name);
    }
}