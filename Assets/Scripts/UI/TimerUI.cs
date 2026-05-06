using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
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
        timerText.text = FormatTime(time);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }
}