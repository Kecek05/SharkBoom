using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, IGameActions
{

    private Controls controls;

    public event Action<InputAction.CallbackContext> OnPrimaryFingerPositionEvent;
    public event Action<InputAction.CallbackContext> OnSecondaryFingerPositionEvent;
    public event Action<InputAction.CallbackContext> OnSecondaryTouchContactEvent;
    public event Action<InputAction.CallbackContext> OnTouchPressEvent;



    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Game.SetCallbacks(this);
        }

        controls.Game.Enable();
    }

    private void OnDisable()
    {
        controls.Game.Disable();
    }

    public void OnPrimaryFingerPosition(InputAction.CallbackContext context)
    {
        OnPrimaryFingerPositionEvent?.Invoke(context);
    }

    public void OnSecondaryFingerPosition(InputAction.CallbackContext context)
    {
        OnSecondaryFingerPositionEvent?.Invoke(context);
    }

    public void OnSecondaryTouchContact(InputAction.CallbackContext context)
    {
        OnSecondaryTouchContactEvent?.Invoke(context);   
    }

    public void OnTouchPress(InputAction.CallbackContext context)
    {
        OnTouchPressEvent?.Invoke(context);
    }
}
