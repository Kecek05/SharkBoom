using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(DamageableSO damageableSO);
}

public interface IDraggable
{
    public void Release(float force, Vector3 direction);
}



public interface IState
{
    public void Enter(); // Code that runs when we first enter the state

    public void Execute(); // per-frame logic, include condition to transition to a new state

    public void Exit(); // Code that runs when we exit the state
}

public interface IActivable
{
    public void Setup();

    public void Activate();
}

public interface IFollowable
{
    public void Follow(Transform target);
}


public interface IServerAuthenticationService //Decopling of the Authentication Services
{
    int RegisteredClientCount { get; }
    void RegisterClient(PlayerData playerData);
    void UnregisterClient(ulong clientId);

    void RegisterPlayableClient(PlayerData playerData);

    public List<PlayerData> PlayerDatas { get; }
    public Dictionary<ulong, PlayerData> ClientIdToPlayerData { get; }
    public Dictionary<string, ulong>.ValueCollection AuthToClientIdValues { get; }
    PlayerData GetPlayerDataByClientId(ulong clientId);
    public string GetAuthIdByClientId(ulong clientId);
    public ulong GetClientIdByAuthId(string authId);
}

public interface IPlayerSpawner //Decopling of the Authentication Services
{
    int PlayerCount { get; }

    void SpawnPlayer(ulong clientId);

    PlayableState GetPlayableStateByCount();
}

public interface IGameStateManager //Decopling GameStateManager
{
    public event Action<GameState> OnGameStateChange;
    public event Action<int> OnCanShowPearls;
    public event Action OnGameOver;
    public event Action OnWin;
    public event Action OnLose;

    /// <summary>
    /// Called when any player lost connection in Host.
    /// </summary>
    public event Action OnLostConnectionInHost;

    /// <summary>
    /// Called when the server should be closed.
    /// </summary>
    public event Action OnCanCloseServer;

    public int DelayClosePlayersInfo { get; }

    public NetworkVariable<GameState> CurrentGameState { get; }

    public void ConnectionLostHostAndClient();
    public void ChangeGameState(GameState gameState, int delayToChange = 0);
}

public interface IGameOverManager //Decopling management of GameOver
{
    public NetworkVariable<PlayableState> LosedPlayer { get; }
    public bool GameOver { get; }
    public void LoseGame(PlayableState playerLosedPlayableState);

    public void GameOverClient();

}