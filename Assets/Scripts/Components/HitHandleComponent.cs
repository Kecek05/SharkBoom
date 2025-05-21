using System;
using UnityEngine;

public class HitHandleComponent : MonoBehaviour, IRecieveHit
{
    public event Action OnHitRecieve;

    public void Hit()
    {
        OnHitRecieve?.Invoke();
    }
}
