using System;
using Unity.Netcode;

public abstract class BaseGameOverManager : NetworkBehaviour
{
    //Events
    public event Action OnWin;
    public event Action<bool> OnLose;


    //Variables

    protected NetworkVariable<PlayableState> losedPlayer = new(PlayableState.None);

    //Publics
    public NetworkVariable<PlayableState> LosedPlayer => losedPlayer;


    //Methods

    protected void TriggerOnWin() => OnWin?.Invoke();

    protected void TriggerOnLose(bool isDraw) => OnLose?.Invoke(isDraw);

    public abstract void DefineTheWinner();

    public abstract void GameOverClient();

    public abstract void HandleOnGameStateChanged(GameState gameState);

    public abstract void HandleOnLosedPlayerChanged(PlayableState newValue);

}
