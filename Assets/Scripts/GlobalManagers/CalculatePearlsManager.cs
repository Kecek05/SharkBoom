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

    private static CalculatedPearls player1CalculatedPearls;
    private static CalculatedPearls player2CalculatedPearls;

    //private static bool alreadyChanged = false;

    //public CalculatePearlsManager(PlayerData player1Data, PlayerData player2Data)
    //{
    //    CalculatePossibleResults(player1Data, player2Data);

    //    GameStateManager.OnGameEndedServer += GameStateManager_OnGameEndedServer;
    //}



    //private void GameStateManager_OnGameEndedServer()
    //{
    //    //ChangePearls
    //}

    public static void CalculatePossibleResults(PlayerData player1Data, PlayerData player2Data)
    {
        //Not calculate in relay

        (int player1CalculatedPearlsToWin, int player2CalculatedPearlsToLose) = CalculateDelta(player1Data.userData.userPearls, player2Data.userData.userPearls);

        (int player2CalculatedPearlsToWin, int player1CalculatedPearlsToLose) = CalculateDelta(player2Data.userData.userPearls, player1Data.userData.userPearls);

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
    /// Call this to change the save of both players
    /// </summary>
    /// <param name="winnerPlayerData"> The Winner</param>
    /// <param name="loserPlayerData"> The Loser</param>
    /// <returns></returns>
    private static async Task ChangePearls(PlayerData winnerPlayerData, PlayerData loserPlayerData)
    {

        await Save.AddSavePlayerPearls(winnerPlayerData.userData.userAuthId, winnerPlayerData.calculatedPearls.PearlsToWin);

        await Save.AddSavePlayerPearls(loserPlayerData.userData.userAuthId, loserPlayerData.calculatedPearls.PearlsToLose);

        Debug.Log($"Changing Pearls of players. Winner: {winnerPlayerData.userData.userName} Wins Pearls: {winnerPlayerData.calculatedPearls.PearlsToWin} - Loser: {loserPlayerData.userData.userName} Loses Pearls: {loserPlayerData.calculatedPearls.PearlsToLose}");

        OnFinishedChangingPearls?.Invoke();
    }

    //public static async Task TriggerChangePearls()
    //{
    //    if (alreadyChanged) return;

    //    if (GameFlowManager.Instance.GameStateManager.LocalWin)
    //    {
    //        OnPearlsDeltaChanged?.Invoke(pearlsToWinDelta);

    //        //Not calculate in relay
    //        if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
    //            await ChangePearls(pearlsIfWin);
    //    }
    //    else
    //    {
    //        OnPearlsDeltaChanged?.Invoke(-pearlsToLoseDelta); //pass a negative value

    //        //Not calculate in relay
    //        if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
    //            await ChangePearls(pearlsIfLose);
    //    }
    //}


    //private static async Task<int> CalculatePearlsToWin(string userAuthId)
    //{
    //    //Calculate win pearls

    //    return UnityEngine.Random.Range(10, 100); //Math of value to win


    //}



    //private static async Task<int> CalculatePearlsIfWin(string userAuthId)
    //{
    //    //Pearls that the player will have if win

    //    return await Save.LoadPlayerPearls(userAuthId) + pearlsToWinDelta;

    //}


    //private static async Task CalculateLosePearls(string userAuthId)
    //{
    //    //Calculate lose pearls

    //    pearlsToLoseDelta = UnityEngine.Random.Range(10, 100); //Math of value to lose  

    //    int result = await Save.LoadPlayerPearls(userAuthId) - pearlsToLoseDelta;

    //    pearlsIfLose = result;

    //    if(pearlsIfLose < 0)
    //    {
    //        pearlsIfLose = 0;
    //    }

    //    Debug.Log($"Lose: {pearlsToLoseDelta} - {pearlsIfLose}");
    //}

    //private static async Task ChangePearls(int pearls)
    //{
    //    //await Save.SavePlayerPearls(pearls);

    //    alreadyChanged = true;

    //    Debug.Log($"Changed Pearls to: {pearls}");
    //}

    //public static void Reset()
    //{
    //    pearlsToWinDelta = 0;
    //    pearlsToLoseDelta = 0;
    //    pearlsIfWin = 0;
    //    pearlsIfLose = 0;
    //    alreadyChanged = false;
    //}
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

    ///// <summary>
    ///// Total of pearls that the player will have if win.
    ///// </summary>
    //public int PearlsIfWin;

    ///// <summary>
    ///// Total of pearls that the player will have if lose.
    ///// </summary>
    //public int PearlsIfLose;
}
