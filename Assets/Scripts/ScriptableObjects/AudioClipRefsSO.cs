using UnityEngine;

[CreateAssetMenu(fileName = "AudioClips", menuName = "Scriptable Objects/AudioClipsSO")]
public class AudioClipRefsSO : ScriptableObject
{
    public AudioClip[] drag;
    public AudioClip[] launchItem;
    public AudioClip[] activeItem;
    public AudioClip[] hitObject;

    public AudioClip[] startYourTurn;
    public AudioClip[] startEnemyTurn;
    public AudioClip[] finishGameTime;
    public AudioClip[] finishTurn;

    public AudioClip[] gameStart;
    public AudioClip[] gameVictory;
    public AudioClip[] gameDefeat;
    public AudioClip[] gameDraw;
}
