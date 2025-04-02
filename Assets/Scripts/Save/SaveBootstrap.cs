using System.Threading.Tasks;
using UnityEngine;

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
            //Is in match, reconnect
            string ipMatch = await Reconnect.GetIpMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId);
            int portMatch = await Reconnect.GetPortMatch(ClientSingleton.Instance.GameManager.UserData.userAuthId);

            ClientSingleton.Instance.GameManager.StartMatchmakingClient(ipMatch, portMatch);

        } else
        {
            //Not in match, go to menu
            Loader.Load(Loader.Scene.MainMenu);
        }
    }
}
