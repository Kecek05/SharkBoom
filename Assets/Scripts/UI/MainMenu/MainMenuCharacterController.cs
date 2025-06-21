using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class MainMenuCharacterController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private VideoClip sharkIdleVideoClip;
    [SerializeField] private VideoClip orcaIdleVideoClip;
    [SerializeField] private VideoPlayer characterVideoPlayer;

    private void Start()
    {
        int randomPlayer = Random.Range(0, 2);
        if (randomPlayer == 0)
        {
            characterVideoPlayer.clip = sharkIdleVideoClip;
        }
        else
        {
            characterVideoPlayer.clip = orcaIdleVideoClip;
        }
    }
}
