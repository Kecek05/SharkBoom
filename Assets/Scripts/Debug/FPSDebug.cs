using System.Collections;
using TMPro;
using UnityEngine;

public class FPSDebug : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    private int fps;
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.5f);

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(CountFps());
    }

    private IEnumerator CountFps()
    {
        while(true)
        {
            yield return waitForSecondsRealtime;
            fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps.ToString();
        }
    }
}
