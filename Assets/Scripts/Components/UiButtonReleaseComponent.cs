using System;
using UnityEngine;
using UnityEngine.Events;

public class UiButtonReleaseComponent : MonoBehaviour
{
    [SerializeField] private GameObject button;
    [SerializeField] private UnityEvent buttonAction;

    public void ButtonRelease()
    {
        if (UIDetection.IsPointerOverThisObject(button))
        {
            buttonAction?.Invoke();
            Debug.Log("Testing");
        }
    }

}
