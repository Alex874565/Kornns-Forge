using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private Color defaultColor;

    private void Start()
    {
        if (timerText != null)
            defaultColor = timerText.color;

        if (KornnGameManager.Instance != null)
        {
            KornnGameManager.Instance.OnTimeChanged += UpdateTimer;
        }
    }

    private void OnDestroy()
    {
        if (KornnGameManager.Instance != null)
        {
            KornnGameManager.Instance.OnTimeChanged -= UpdateTimer;
        }
    }

    private void UpdateTimer(float time)
    {
        if (timerText == null) return;

        timerText.text = FormatTime(time);

        if (time <= 60f)
            timerText.color = Color.red;
        else
            timerText.color = defaultColor;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }
}