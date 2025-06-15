using UnityEngine;
using DG.Tweening;
using Sortify;
using UnityEngine.Events;

public class UiAnimator : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [BetterHeader("Settings")]
    [SerializeField] private float fadeTime;
    [SerializeField] private float startPanelScale;
    [SerializeField] private float endPanelScale;
    [SerializeField] private Vector2 moveStart;
    [SerializeField] private Vector2 moveEnd;
    [SerializeField] private Ease fadeInAnimation;

    public UnityEvent OnUiAnimation;

    private void OnEnable()
    {
        OnUiAnimation?.Invoke();
    }

    public void ChangeTransparency()
    {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, fadeTime);
    }

    public void ChangeScale()
    {
        transform.localScale = new Vector3(startPanelScale, startPanelScale, startPanelScale);
        transform.DOScale(new Vector3(endPanelScale, endPanelScale, endPanelScale), fadeTime).SetEase(fadeInAnimation);
    }

    public void ChangeMove()
    {
        rectTransform.anchoredPosition = moveStart;
        rectTransform.DOAnchorPos(moveEnd, fadeTime).SetEase(fadeInAnimation);
    }
}
