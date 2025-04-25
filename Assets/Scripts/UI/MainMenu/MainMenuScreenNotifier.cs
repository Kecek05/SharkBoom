using UnityEngine;

public class MainMenuScreenNotifier : MonoBehaviour
{
    [SerializeField] private MainMenuCharacterController mainMenuCharacter;

    private void OnDisable()
    {
        mainMenuCharacter.Character.SetActive(true);
    }

    private void OnEnable()
    {
        mainMenuCharacter.Character.SetActive(false);
    }

}
