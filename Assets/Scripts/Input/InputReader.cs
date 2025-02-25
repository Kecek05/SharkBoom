using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, IGameActions
{

    private Controls controls;

    public event Action<InputAction.CallbackContext> OnTouchEvent;



    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Game.SetCallbacks(this);
        }

        controls.Game.Enable();
    }

    public void OnTouch(InputAction.CallbackContext context)
    {
        OnTouchEvent?.Invoke(context);
    }

}
