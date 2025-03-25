using System;
using System.Threading.Tasks;
using UnityEngine;

public static class CalculatePearlsManager
{
    public static event Action<int> OnPearlsDeltaChanged;
    public static event Action OnFinishedCalculateResults;

    //Value that will be changed, count the pearls that the player will win or lose.
    private static int pearlsToWinDelta = 0;
    private static int pearlsToLoseDelta = 0;

    //Value that will be saved, count the pearls that the player already have.
    private static int pearlsIfWin = 0;
    private static int pearlsIfLose = 0;


    private static bool alreadyChanged = false;

    public static async void CalculatePossibleResults()
    {
        //Not calculate in relay
        if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
        {
            await CalculateWinPearls();
            await CalculateLosePearls();
        }

        OnFinishedCalculateResults?.Invoke();
    }

    public static async Task TriggerChangePearls()
    {
        if (alreadyChanged) return;

        if (GameFlowManager.Instance.GameStateManager.LocalWin)
        {
            OnPearlsDeltaChanged?.Invoke(pearlsToWinDelta);

            //Not calculate in relay
            if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
                await ChangePearls(pearlsIfWin);
        }
        else
        {
            OnPearlsDeltaChanged?.Invoke(-pearlsToLoseDelta); //pass a negative value

            //Not calculate in relay
            if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
                await ChangePearls(pearlsIfLose);
        }
    }


    private static async Task CalculateWinPearls()
    {
        //Calculate win pearls

        pearlsToWinDelta = UnityEngine.Random.Range(10, 100); //Math of value to win

        int result = await Save.LoadPlayerPearls() + pearlsToWinDelta;

        pearlsIfWin = result;

        Debug.Log($"Win: {pearlsToWinDelta} - {pearlsIfWin}");
    }


    private static async Task CalculateLosePearls()
    {
        //Calculate lose pearls

        pearlsToLoseDelta = UnityEngine.Random.Range(10, 100); //Math of value to lose  

        int result = await Save.LoadPlayerPearls() - pearlsToLoseDelta;

        pearlsIfLose = result;

        if(pearlsIfLose < 0)
        {
            pearlsIfLose = 0;
        }

        Debug.Log($"Lose: {pearlsToLoseDelta} - {pearlsIfLose}");
    }

    private static async Task ChangePearls(int pearls)
    {
        await Save.SavePlayerPearls(pearls);

        alreadyChanged = true;

        Debug.Log($"Changed Pearls to: {pearls}");
    }

    public static void Reset()
    {
        pearlsToWinDelta = 0;
        pearlsToLoseDelta = 0;
        pearlsIfWin = 0;
        pearlsIfLose = 0;
        alreadyChanged = false;
    }
}
