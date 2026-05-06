using TMPro;
using UnityEngine;
using Unity.Netcode;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        ScoreManager.Instance.GetScoreVariable().OnValueChanged += OnScoreChanged;

        UpdateScore(ScoreManager.Instance.GetScore());
    }

    private void OnScoreChanged(int oldValue, int newValue)
    {
        UpdateScore(newValue);
    }

    private void UpdateScore(int value)
    {
        scoreText.text = "Score: " + value;
    }
}