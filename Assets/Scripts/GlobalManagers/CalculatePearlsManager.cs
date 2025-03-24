using System;
using System.Threading.Tasks;
using UnityEngine;

public static class CalculatePearlsManager
{
    public static event Action<int> OnPearlsDeltaChanged;

    //Value that will be changed, count the pearls that the player will win or lose.
    private static int pearlsToWinDelta = 0;
    private static int pearlsToLoseDelta = 0;

    //Value that will be saved, count the pearls that the player already have.
    private static int pearlsIfWin = 0;
    private static int pearlsIfLose = 0;


    private static bool alreadyChanged = false;

    public static async void CalculatePossibleResults()
    {
        await CalculateWinPearls();
        await CalculateLosePearls();
    }

    public static async Task TriggerChangePearls(bool win)
    {
        if (win)
        {
            OnPearlsDeltaChanged?.Invoke(pearlsToWinDelta);
            await ChangePearls(pearlsIfWin);
        }
        else
        {
            OnPearlsDeltaChanged?.Invoke(pearlsIfLose);
            await ChangePearls(pearlsIfLose);
        }
    }


    private static async Task CalculateWinPearls()
    {
        //Calculate win pearls

        pearlsToWinDelta = UnityEngine.Random.Range(10, 100); //Math of value to win

        int result = await Save.LoadPlayerPearls() + pearlsToWinDelta;

        pearlsIfWin = result;
    }


    private static async Task CalculateLosePearls()
    {
        //Calculate lose pearls

        pearlsToLoseDelta = UnityEngine.Random.Range(10, 100); //Math of value to lose  

        int result = await Save.LoadPlayerPearls() - pearlsToLoseDelta;

        pearlsIfLose = result;
    }

    private static async Task ChangePearls(int pearls)
    {
        if (alreadyChanged) return;

        await Save.SavePlayerPearls(pearls);

        alreadyChanged = true;
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
