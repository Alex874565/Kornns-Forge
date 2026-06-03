using UnityEngine;
using DG.Tweening;

public class UIWarning : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Flicker")]
    [SerializeField] private float warningDuration = 5f;
    [SerializeField] private float startBlinkDuration = 0.8f;
    [SerializeField] private float endBlinkDuration = 0.05f;
    [SerializeField] private float minAlpha = 0.15f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Sequence flickerSequence;
    private float elapsedTime;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ShowFlicker()
    {
        elapsedTime = 0f;

        flickerSequence?.Kill();

        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        spriteRenderer
            .DOFade(1f, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(StartBlink);
    }

    private void StartBlink()
    {
        flickerSequence?.Kill();

        float t = Mathf.Clamp01(elapsedTime / warningDuration);
        float curveT = speedCurve.Evaluate(t);

        float currentDuration = Mathf.Lerp(
            startBlinkDuration,
            endBlinkDuration,
            curveT
        );

        flickerSequence = DOTween.Sequence();

        flickerSequence.Append(spriteRenderer.DOFade(minAlpha, currentDuration));
        flickerSequence.Append(spriteRenderer.DOFade(1f, currentDuration));

        flickerSequence.OnComplete(() =>
        {
            elapsedTime += currentDuration * 2f;
            StartBlink();
        });
    }

    public void Hide()
    {
        flickerSequence?.Kill();
        flickerSequence = null;

        spriteRenderer
            .DOFade(0f, 0.15f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        flickerSequence?.Kill();
    }
}