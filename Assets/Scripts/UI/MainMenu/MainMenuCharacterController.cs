using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class MainMenuCharacterController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private VideoClip sharkIdleVideoClip;
    [SerializeField] private VideoClip orcaIdleVideoClip;
    [SerializeField] private VideoPlayer characterVideoPlayer;
    [SerializeField] private GameObject loadingPanel;

    
    private void Awake()
    {
        loadingPanel.SetActive(true);
        characterVideoPlayer.prepareCompleted += HandleOnVideoPlayerPrepared;
        
        int randomPlayer = Random.Range(0, 2);

        if (randomPlayer == 0)
        {
            characterVideoPlayer.clip = sharkIdleVideoClip;
            characterVideoPlayer.Prepare();
        }
        else
        {
            characterVideoPlayer.clip = orcaIdleVideoClip;
            characterVideoPlayer.Prepare();
        }
    }

    private void HandleOnVideoPlayerPrepared(VideoPlayer source)
    {
        // Debug.Log("Teste");
        source.Play();
        loadingPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        characterVideoPlayer.prepareCompleted -= HandleOnVideoPlayerPrepared;
    }
}
