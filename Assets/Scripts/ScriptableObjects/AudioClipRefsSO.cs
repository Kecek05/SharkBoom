using UnityEngine;

[CreateAssetMenu(fileName = "AudioClips", menuName = "Scriptable Objects/AudioClipsSO")]
public class AudioClipRefsSO : ScriptableObject
{
    // Warnings
    public AudioClip[] startYourTurn;
    public AudioClip[] startEnemyTurn;
    public AudioClip[] finishTurn;

    // GameStates
    public AudioClip[] gameStart;
    public AudioClip[] gameEnd;
    public AudioClip[] gameWin;
    public AudioClip[] gameLose;
    public AudioClip[] gameTie;

    // Player Actions
    public AudioClip[] drag;
    public AudioClip[] launchItem;
    public AudioClip[] activeItem;
    public AudioClip[] hitObject;
}
