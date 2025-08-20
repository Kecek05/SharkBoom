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
        //Check if player can safely reconnect to a match
        if (await Reconnect.CanSafelyReconnect(ClientSingleton.Instance.GameManager.UserData.userAuthId))
        {
            //All checks passed - reconnect to the match
            string ipMatch = await Reconnect.GetIpMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId);
            int portMatch = await Reconnect.GetPortMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId);

            Debug.Log($"Match validation passed, reconnecting to {ipMatch}:{portMatch}");
            ClientSingleton.Instance.GameManager.StartMatchmakingClient(ipMatch, portMatch);
        } 
        else
        {
            //Not in match or reconnect validation failed, go to Name Bootstrap
            Debug.Log("No valid match to reconnect to, proceeding to menu");
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