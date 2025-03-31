using Unity.Netcode;

public class GameOverManager : BaseGameOverManager
{
    
    private BaseTurnManager turnManager;

    private void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
    }

    public override void GameOverClient()
    {
        if (losedPlayer.Value == turnManager.LocalPlayableState)
        {
            //Change pearls, then lose


            //await CalculatePearlsManager.TriggerChangePearls();
            TriggerOnLose();
        }
        else if (losedPlayer.Value == PlayableState.None)
        {
            //Tie, lose


            //await CalculatePearlsManager.TriggerChangePearls();
            TriggerOnLose();
        }
        else
        {
            //Change pearls, then win

            //await CalculatePearlsManager.TriggerChangePearls();
            TriggerOnWin();
        }
    }

    public override void LoseGame(PlayableState playerLosedPlayableState)
    {
        losedPlayer.Value = playerLosedPlayableState;
    }

    public override void HandleOnLosedPlayerValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        TriggerOnGameOver();

        if (IsClient)
        {
            GameOverClient();
        }

        gameOver = true;
    }

}
