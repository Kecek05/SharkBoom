using UnityEngine;

public class SpawnObjectOnTouchDebug : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;

    [SerializeField] private InputReader inputReader;

    private void Start()
    {
        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
    }

    private void InputReader_OnTouchPressEvent(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Instantiate(objectToSpawn, obj.ReadValue<Vector2>(), Quaternion.identity);
        Debug.Log("Touch Pressed, Object Spawned");
    }
}
