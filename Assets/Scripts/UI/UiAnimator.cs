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
    [SerializeField] private float startPanelScale;
    [SerializeField] private float endPanelScale;
    [SerializeField] private Ease fadeInAnimation;


    private void OnEnable()
    {
        PanelFadeIn();
    }

    private void PanelFadeIn()
    {
        canvasGroup.alpha = 0;
        transform.localScale = new Vector3(startPanelScale, startPanelScale, startPanelScale);
        transform.DOScale(new Vector3(endPanelScale, endPanelScale, endPanelScale), fadeTime).SetEase(fadeInAnimation);
        canvasGroup.DOFade(1, fadeTime);
    }
}
