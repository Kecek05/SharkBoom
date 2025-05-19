using System;
using UnityEngine;

public class HitRecieveComponent : MonoBehaviour, IRecieveHit
{
    public event Action OnHitRecieve;

    public void Hit()
    {
        OnHitRecieve?.Invoke();
    }
}
