using UnityEngine;
using DG.Tweening;
using Sortify;

public class UiAnimator : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [BetterHeader("Settings")]
    [SerializeField] private float fadeTime;
    [SerializeField] private float distanceForMovePanel;
    [SerializeField] private Ease fadeInAnimation;
    [SerializeField] private Ease fadeOutAnimation;


    private void OnEnable()
    {
        PanelFadeIn();
        Debug.Log("FadeIn");
    }

    private void OnDisable()
    {
        PanelFadeOut();
        Debug.Log("FadeOut");
    }

    private void PanelFadeIn()
    {
        canvasGroup.alpha = 0;
        rectTransform.transform.localPosition = new Vector3(0f, -distanceForMovePanel, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(fadeInAnimation);
        canvasGroup.DOFade(1, fadeTime);
    }

    private void PanelFadeOut()
    {
        canvasGroup.alpha = 1;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, -distanceForMovePanel), fadeTime, false).SetEase(fadeOutAnimation);
        canvasGroup.DOFade(0, fadeTime);
    }
}
