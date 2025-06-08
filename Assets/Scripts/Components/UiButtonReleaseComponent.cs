using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiButtonReleaseComponent : MonoBehaviour
{
    [SerializeField] private GameObject gameobject;
    [SerializeField] private Button button;
    [SerializeField] private UnityEvent buttonAction;


    public void ButtonRelease()
    {
        if (UIDetection.IsPointerOverThisObject(gameobject) && button.interactable)
        {
            buttonAction?.Invoke();
        }
    }

}
