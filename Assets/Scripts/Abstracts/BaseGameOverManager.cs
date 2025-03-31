using System;
using Unity.Netcode;

public abstract class BaseGameOverManager : NetworkBehaviour
{
    //Events
    public event Action<int> OnCanShowPearls;
    public event Action OnGameOver;
    public event Action OnWin;
    public event Action OnLose;

    //Variables

    protected NetworkVariable<PlayableState> losedPlayer = new(PlayableState.None);

    //Publics
    public NetworkVariable<PlayableState> LosedPlayer => losedPlayer;


    //Methods
    protected void TriggerOnGameOver() => OnGameOver?.Invoke();

    protected void TriggerOnWin() => OnWin?.Invoke();

    protected void TriggerOnLose() => OnLose?.Invoke();

    /// <summary>
    /// Call this to know who loses and who wins. Server is the only who call this.
    /// </summary>
    public abstract void LoseGame();

    public abstract void DefineTheWinner();

    public abstract void GameOverClient();

}
