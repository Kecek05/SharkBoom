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

    // GameOver
    public AudioClip[] gameWin;
    public AudioClip[] gameLose;
    public AudioClip[] gameTie;

    // Item Actions
    public AudioClip[] itemSpawn;
    public AudioClip[] itemDespawn;
    public AudioClip[] itemLaunched;
    public AudioClip[] itemActivable;
    public AudioClip[] itemHit;
}
