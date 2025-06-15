using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "TutorialInfoSO", menuName = "Scriptable Objects/TutorialInfoSO")]
public class TutorialInfoSO : ScriptableObject
{
    public string tutorialTitle;
    public VideoClip tutorialVideo;
    public int itemId;
}
