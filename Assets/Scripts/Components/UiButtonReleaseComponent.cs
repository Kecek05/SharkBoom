using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UiButtonReleaseComponent : MonoBehaviour
{
    [SerializeField] private GameObject buttonGameobject;
    [SerializeField] private Button button;
    [SerializeField] private UnityEvent buttonAction;

    private void Awake()
    {
        button.clicked += ButtonRelease;
    }

    public void ButtonRelease()
    {
        if (UIDetection.IsPointerOverThisObject(buttonGameobject))
        {
            buttonAction?.Invoke();
            Debug.Log("Testing");
        }
    }

}
