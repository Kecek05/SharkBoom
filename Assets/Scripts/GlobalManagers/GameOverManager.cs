using Unity.Netcode;

public class GameOverManager : BaseGameOverManager
{
    
    private BaseTurnManager turnManager;
    private BasePlayersPublicInfoManager playersPublicInfoManager;

    private void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
    }

    public override void GameOverClient()
    {
        if (losedPlayer.Value == turnManager.LocalPlayableState)
        {
            TriggerOnLose();
        }
        else if (losedPlayer.Value == PlayableState.Tie)
        {
            TriggerOnLose();
        }
        else
        {
            TriggerOnWin();
        }
    }

    public override void DefineTheWinner()
    {
        // Calculate the winner of the game
        //Code to check if both players have the same health, if so, tie, otherwise check who has the most health and declare the winner.

        if (!IsServer) return;

        PlayerHealth player1Health = playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player1Playing).GetComponent<PlayerHealth>();

        PlayerHealth player2Health = playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player2Playing).GetComponent<PlayerHealth>();

        if (player1Health.CurrentHealth.Value == player2Health.CurrentHealth.Value)
        {
            //Tie
            losedPlayer.Value = PlayableState.Tie;
        }
        else if (player1Health.CurrentHealth.Value > player2Health.CurrentHealth.Value)
        {
            //Player 2 loses
            losedPlayer.Value = PlayableState.Player2Playing;
        }
        else
        {
            //Player 1 loses
            losedPlayer.Value = PlayableState.Player1Playing;
        }
    }

    public override void HandleOnLosedPlayerChanged(PlayableState newValue)
    {
        if(IsClient)
        {
            GameOverClient();
        }
    }


    public override void HandleOnGameStateChanged(GameState gameState)
    {
        if(gameState == GameState.GameEnded)
        {
            DefineTheWinner();
        }
    }
}
