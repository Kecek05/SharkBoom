using UnityEngine;

public class GameTutorialHandler : MonoBehaviour
{
    public void FinishedTutorial()
    {
        Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
    }
}
