using Sortify;
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
    public PlayerState State { get; } // The state enum that this state represents
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
    void RegisterUserData(UserData userData, ulong clientId);
    public List<PlayerData> PlayerDatas { get; }
    public Dictionary<ulong, PlayerData> ClientIdToPlayerData { get; }
    public Dictionary<ulong, string> ClientIdToAuth { get; }
    public Dictionary<string, ulong> AuthIdToClientId { get; }
    public Dictionary<string, PlayerData> AuthIdToPlayerData { get; }
    public Dictionary<ulong, UserData> ClientIdToUserData { get; }
    public PlayerData GetPlayerDataByClientId(ulong clientId);
    public PlayerData GetPlayerDataByAuthId(string authId);
    public string GetAuthIdByClientId(ulong clientId);
    public ulong GetClientIdByAuthId(string authId);
}

public interface IPlayerSpawner
{
    int PlayerCount { get; }

    void SpawnPlayer();

    PlayableState GetPlayableStateByCount();
}