using System;
using UnityEngine;

public class HitRecieve : MonoBehaviour, IRecieveHit
{
    public event Action OnHitRecieve;

    public void Hit()
    {
        OnHitRecieve?.Invoke();
    }
}
