using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;
    [SerializeField] private Transform mainCamera;

    private void Start()
    {
        YourTurnUI.OnYourTurnSound += PlayYourTurnSound;
        TimersUpUI.OnTurnTimersUpSound += PlayTurnTimersUpSound;
    }

    private void PlayYourTurnSound()
    {
        PlaySound(audioClipRefsSO.startYourTurn, mainCamera.position);
    }
    private void PlayTurnTimersUpSound()
    {
        PlaySound(audioClipRefsSO.finishTurn, mainCamera.position);
    }

    public static void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    { 
        AudioSource.PlayClipAtPoint(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }
}


