using System;
using Unity.Netcode;

public abstract class BaseGameOverManager : NetworkBehaviour
{
    //Events
    public event Action OnGameOver;
    public event Action OnWin;
    public event Action OnLose;

    //Variables

    protected NetworkVariable<PlayableState> losedPlayer = new(PlayableState.None);
    protected bool gameOver = false;

    //Publics
    public NetworkVariable<PlayableState> LosedPlayer => losedPlayer;
    public bool GameOver => gameOver;


    //Methods
    protected void TriggerOnGameOver() => OnGameOver?.Invoke();

    protected void TriggerOnWin() => OnWin?.Invoke();

    protected void TriggerOnLose() => OnLose?.Invoke();


    /// <summary>
    /// Call this to know who loses and who wins. Server is the only who call this.
    /// </summary>
    /// <param name="playerLosedPlayableState"> Playing State of the player who loses</param>
    public abstract void LoseGame(PlayableState playerLosedPlayableState);


    public abstract void GameOverClient();

    public abstract void HandleOnLosedPlayerValueChanged(PlayableState previousValue, PlayableState newValue);

    public abstract void HandleOnGameStateChanged(GameState newValue);
    protected abstract void CheckForTheWinner();

    protected abstract void SendGameResultsToClient(string authId, int valueToShow);

}
