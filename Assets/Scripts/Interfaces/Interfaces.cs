using System.Collections.Generic;
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
    void RegisterClient(ulong clientId, UserData userData);
    void UnregisterClient(ulong clientId);
    int RegisteredClientCount { get; }
    public Dictionary<string, ulong>.ValueCollection AuthToClientIdValues { get; }
    UserData GetUserDataByClientId(ulong clientId);
    public string GetAuthIdByClientId(ulong clientId);
    public ulong GetClientIdByAuthId(string authId);
}

public interface IPlayerSpawner //Decopling of the Authentication Services
{
    void SpawnPlayer(ulong clientId, PlayableState playerState);
}