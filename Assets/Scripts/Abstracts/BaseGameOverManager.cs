using System;
using Unity.Netcode;

public abstract class BaseGameOverManager : NetworkBehaviour
{
    //Events
    public event Action<int> OnCanShowPearls;
    public event Action OnWin;
    public event Action OnLose;

    //Variables

    protected NetworkVariable<PlayableState> losedPlayer = new(PlayableState.None);

    //Publics
    public NetworkVariable<PlayableState> LosedPlayer => losedPlayer;


    //Methods

    protected void TriggerOnWin() => OnWin?.Invoke();

    protected void TriggerOnLose() => OnLose?.Invoke();

    public abstract void DefineTheWinner();

    public abstract void GameOverClient();

    public abstract void HandleOnGameStateChange(GameState gameState);

    public abstract void HandleOnLosedPlayerChanged(PlayableState newValue);

}
