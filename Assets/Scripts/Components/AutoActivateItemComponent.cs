using System;
using UnityEngine;

public class AutoActivateItemComponent : MonoBehaviour
{
    public event Action OnActivate;
    
    public void SelfActivate()
    {
        OnActivate?.Invoke();
    }
}
