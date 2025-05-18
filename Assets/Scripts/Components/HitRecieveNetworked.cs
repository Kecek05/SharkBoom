using System;
using UnityEngine;

public class HitRecieveNetworked : MonoBehaviour, IRecieveHit
{
    [SerializeField] private PlayerGetUp playerGetUp;

    public void Hit()
    {
        playerGetUp.HitRecieveNotify();
        Debug.Log("Hit trigger - Send the hit recieve notify");
    }
}
