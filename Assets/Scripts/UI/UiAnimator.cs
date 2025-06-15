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
    [SerializeField] private Vector3 movePosition;
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
        rectTransform.transform.localPosition = movePosition;
        rectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(fadeInAnimation);
    }
}
