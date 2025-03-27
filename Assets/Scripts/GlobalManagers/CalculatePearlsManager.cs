using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class CalculatePearlsManager
{
    //public static event Action<int> OnPearlsDeltaChanged;
    //public static event Action OnFinishedCalculateResults;

    /// <summary>
    /// Called when the predicted results has been calculated.
    /// </summary>
    public static event Action OnFinishedCalculatePossibleResults;

    /// <summary>
    /// Called when finished changing the save of both players.
    /// </summary>
    public static event Action OnFinishedChangingPearls;

    public static void CalculatePossibleResults(PlayerData player1Data, PlayerData player2Data)
    {
        //Not calculate in relay

        (int player1CalculatedPearlsToWin, int player2CalculatedPearlsToLose) = CalculateDelta(player1Data.userData.UserPearls, player2Data.userData.UserPearls);

        (int player2CalculatedPearlsToWin, int player1CalculatedPearlsToLose) = CalculateDelta(player2Data.userData.UserPearls, player1Data.userData.UserPearls);

        player1Data.calculatedPearls = new CalculatedPearls
        {
            PearlsToWin = player1CalculatedPearlsToWin,
            PearlsToLose = player1CalculatedPearlsToLose,
        };

        player2Data.calculatedPearls = new CalculatedPearls
        {
            PearlsToWin = player2CalculatedPearlsToWin,
            PearlsToLose = player2CalculatedPearlsToLose,
        };

        OnFinishedCalculatePossibleResults?.Invoke();
    }

    private static (int pearlsToWin, int pearlsToLose) CalculateDelta(int winnerPearls, int loserPearls)
    {

        int basePearlsToWin = 100;
        float diffPercentage = (float)(winnerPearls - loserPearls) / loserPearls;


        float winMultiplier = 1.0f;
        float loseMultiplier = 1.0f;

        if (diffPercentage <= -0.6f) { winMultiplier = 1.4f; loseMultiplier = 1.8f; }
        else if (diffPercentage <= -0.4f) { winMultiplier = 1.2f; loseMultiplier = 1.6f; }
        else if (diffPercentage <= -0.2f) { winMultiplier = 1.1f; loseMultiplier = 1.3f; }
        else if (diffPercentage >= 0.6f) { winMultiplier = 0.6f; loseMultiplier = 0.4f; }
        else if (diffPercentage >= 0.4f) { winMultiplier = 0.8f; loseMultiplier = 0.6f; }
        else if (diffPercentage >= 0.2f) { winMultiplier = 0.9f; loseMultiplier = 0.8f; }

        int pearlsToWin = Mathf.RoundToInt(basePearlsToWin * winMultiplier);
        int pearlsToLose = Mathf.RoundToInt(basePearlsToWin * loseMultiplier) * -1;

        Debug.Log($"Win: {pearlsToWin} - Lose: {pearlsToLose}");

        return (pearlsToWin, pearlsToLose);

    }


    /// <summary>
    /// Call this to player loses pearls
    /// </summary>
    /// <param name="loserPlayerData"></param>
    /// <returns></returns>
    public static async Task ChangePearlsLoser(PlayerData loserPlayerData)
    {

        await Save.AddSavePlayerPearls(loserPlayerData.userData.userAuthId, loserPlayerData.calculatedPearls.PearlsToLose);

        Debug.Log($"Changing Pearls of players. Loser: {loserPlayerData.userData.userName} Loses Pearls: {loserPlayerData.calculatedPearls.PearlsToLose}");

        OnFinishedChangingPearls?.Invoke();
    }


    /// <summary>
    /// Call this to player wins pearls
    /// </summary>
    /// <param name="winnerPlayerData"></param>
    /// <returns></returns>
    public static async Task ChangePearlsWinner(PlayerData winnerPlayerData)
    {

        await Save.AddSavePlayerPearls(winnerPlayerData.userData.userAuthId, winnerPlayerData.calculatedPearls.PearlsToWin);

        Debug.Log($"Changing Pearls of players. Winner: {winnerPlayerData.userData.userName} Wins Pearls: {winnerPlayerData.calculatedPearls.PearlsToWin}");

        OnFinishedChangingPearls?.Invoke();
    }

}

public struct CalculatedPearls
{
    /// <summary>
    /// Value of pearls that the player will win.
    /// </summary>
    public int PearlsToWin;

    /// <summary>
    /// Value of pearls that the player will lose.
    /// </summary>
    public int PearlsToLose;

}
