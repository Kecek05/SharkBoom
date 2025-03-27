using System;
using System.Threading.Tasks;
using UnityEngine;

public static class CalculatePearlsManager
{
    public static event Action<int> OnPearlsDeltaChanged;
    public static event Action OnFinishedCalculateResults;


    private static PlayerCalculatedPearls player1CalculatedPearls;
    private static PlayerCalculatedPearls player2CalculatedPearls;

    private static bool alreadyChanged = false;

    public static async void CalculatePossibleResults(string player1AuthId, string player2AuthId)
    {
        //Not calculate in relay

        (int player1CalculatedPearlsToWin, int player2CalculatedPearlsToLose) = CalculateDelta(await Save.LoadPlayerPearls(player1AuthId), await Save.LoadPlayerPearls(player2AuthId));

        (int player2CalculatedPearlsToWin, int player1CalculatedPearlsToLose) = CalculateDelta(await Save.LoadPlayerPearls(player2AuthId), await Save.LoadPlayerPearls(player1AuthId));

        (int player1PearlsIfWin, int player1PearlsIfLose) = await CalculateTotal(player1AuthId, player1CalculatedPearlsToWin, player1CalculatedPearlsToLose);

        (int player2PearlsIfWin, int player2PearlsIfLose) = await CalculateTotal(player2AuthId, player2CalculatedPearlsToWin, player2CalculatedPearlsToLose);

        player1CalculatedPearls = new PlayerCalculatedPearls
        {
            PlayerAuthId = player1AuthId,
            PearlsToWin = player1CalculatedPearlsToWin,
            PearlsToLose = player1CalculatedPearlsToLose,
            PearlsIfWin = player1PearlsIfWin,
            PearlsIfLose = player1PearlsIfLose,
        };

        player2CalculatedPearls = new PlayerCalculatedPearls
        {
            PlayerAuthId = player2AuthId,
            PearlsToWin = player2CalculatedPearlsToWin,
            PearlsToLose = player2CalculatedPearlsToLose,
            PearlsIfWin = player2PearlsIfWin,
            PearlsIfLose = player2PearlsIfLose,
        };


        //if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
        //{
        //    await CalculateWinPearls();
        //    await CalculateLosePearls();
        //}

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


    private static async Task<int> CalculatePearlsToWin(string userAuthId)
    {
        //Calculate win pearls

        return UnityEngine.Random.Range(10, 100); //Math of value to win


    }

    private static (int pearlsToWin, int pearlsToLose) CalculateDelta(int winnerPearls, int loserPearls)
    {
        //int winnerPearls = await Save.LoadPlayerPearls(winnerAuthId);

        //int loserPearls = await Save.LoadPlayerPearls(loserAuthId);

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

    private static async Task<(int pearlsIfWin, int pearlsIfLose)> CalculateTotal(string playerAuthId, int playerCalculatedPearlsToWin, int playerCalculatedPearlsToLose)
    {
        int currentPearls = await Save.LoadPlayerPearls(playerAuthId);

        return (currentPearls + playerCalculatedPearlsToWin, currentPearls + playerCalculatedPearlsToLose);
    }

    private static async Task<int> CalculatePearlsIfWin(string userAuthId)
    {
        //Pearls that the player will have if win

        return await Save.LoadPlayerPearls(userAuthId) + pearlsToWinDelta;

    }


    private static async Task CalculateLosePearls(string userAuthId)
    {
        //Calculate lose pearls

        pearlsToLoseDelta = UnityEngine.Random.Range(10, 100); //Math of value to lose  

        int result = await Save.LoadPlayerPearls(userAuthId) - pearlsToLoseDelta;

        pearlsIfLose = result;

        if(pearlsIfLose < 0)
        {
            pearlsIfLose = 0;
        }

        Debug.Log($"Lose: {pearlsToLoseDelta} - {pearlsIfLose}");
    }

    private static async Task ChangePearls(int pearls)
    {
        //await Save.SavePlayerPearls(pearls);

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

public struct PlayerCalculatedPearls
{

    public string PlayerAuthId;
    /// <summary>
    /// Value of pearls that the player will win.
    /// </summary>
    public int PearlsToWin;

    /// <summary>
    /// Value of pearls that the player will lose.
    /// </summary>
    public int PearlsToLose;

    /// <summary>
    /// Total of pearls that the player will have if win.
    /// </summary>
    public int PearlsIfWin;

    /// <summary>
    /// Total of pearls that the player will have if lose.
    /// </summary>
    public int PearlsIfLose;
}
