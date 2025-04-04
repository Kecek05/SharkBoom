using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class CalculatePearls
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

    private static Dictionary<string, CalculatedPearls> authIdToCalculatedPearls = new Dictionary<string, CalculatedPearls>();

    public static Dictionary<string, CalculatedPearls> AuthIdToCalculatedPearls => authIdToCalculatedPearls;


    public static void CalculatePossibleResults(PlayerData player1Data, PlayerData player2Data)
    {
        //Not calculate in relay

        //(int player1CalculatedPearlsToWin, int player2CalculatedPearlsToLose) = CalculateDelta(player1Data.userData.userPearls, player2Data.userData.userPearls);

        //(int player2CalculatedPearlsToWin, int player1CalculatedPearlsToLose) = CalculateDelta(player2Data.userData.userPearls, player1Data.userData.userPearls);

        //player1Data.calculatedPearls = new CalculatedPearls
        //{
        //    PearlsToWin = player1CalculatedPearlsToWin,
        //    PearlsToLose = player1CalculatedPearlsToLose,
        //};

        //player2Data.calculatedPearls = new CalculatedPearls
        //{
        //    PearlsToWin = player2CalculatedPearlsToWin,
        //    PearlsToLose = player2CalculatedPearlsToLose,
        //};

        OnFinishedCalculatePossibleResults?.Invoke();
    }

    public static void CalculatePossibleResultsWihAllocation(string player1AuthId, string player2AuthId, int player1Pearls, int player2Pearls)
    {
        //Not calculate in relay

        CalculatedPearls player1Win = CalculateDelta(player1Pearls, player2Pearls);

        CalculatedPearls player2Win = CalculateDelta(player2Pearls, player1Pearls);

        authIdToCalculatedPearls[player1AuthId] = new()
        {
            PearlsToWin = player1Win.PearlsToWin,
            PearlsToLose = player2Win.PearlsToLose,
        };

        authIdToCalculatedPearls[player2AuthId] = new()
        {
            PearlsToWin = player2Win.PearlsToWin,
            PearlsToLose = player1Win.PearlsToLose,
        };

        Debug.Log($"Possible Results calculated. Player 1: {player1AuthId} - Pearls to Win: {player1Win.PearlsToWin} - Pearls to Lose: {player2Win.PearlsToLose} Player 2: {player2AuthId} - Pearls to Win: {player2Win.PearlsToWin} - Pearls to Lose: {player1Win.PearlsToLose}");
    }



    private static CalculatedPearls CalculateDelta(int winnerPearls, int loserPearls)
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



        return new(){
            PearlsToWin = pearlsToWin,
            PearlsToLose = pearlsToLose
        };

    }


    /// <summary>
    /// Call this to player loses pearls
    /// </summary>
    /// <param name="loserPlayerData"></param>
    /// <returns></returns>
    public static async Task ChangePearlsLoser(PlayerData loserPlayerData)
    {

        await Save.AddSavePlayerPearls(loserPlayerData.userData.userAuthId, authIdToCalculatedPearls[loserPlayerData.userData.userAuthId].PearlsToLose);

        Debug.Log($"Changing Pearls of players. Loser: {loserPlayerData.userData.userName} Loses Pearls: {authIdToCalculatedPearls[loserPlayerData.userData.userAuthId].PearlsToLose}");

        OnFinishedChangingPearls?.Invoke();
    }


    /// <summary>
    /// Call this to player wins pearls
    /// </summary>
    /// <param name="winnerPlayerData"></param>
    /// <returns></returns>
    public static async Task ChangePearlsWinner(PlayerData winnerPlayerData)
    {

        await Save.AddSavePlayerPearls(winnerPlayerData.userData.userAuthId, authIdToCalculatedPearls[winnerPlayerData.userData.userAuthId].PearlsToLose);

        Debug.Log($"Changing Pearls of players. Winner: {winnerPlayerData.userData.userName} Wins Pearls: {authIdToCalculatedPearls[winnerPlayerData.userData.userAuthId].PearlsToLose}");

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
