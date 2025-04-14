using UnityEngine;

public class TestingFieldServiceLocatorBootstrap : MonoBehaviour
{
    private void Awake()
    {

        BaseTurnManager turnManager = gameObject.AddComponent<TestingFieldTurnManager>();

        ServiceLocator.Register(turnManager);
    }
}
