using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    private void Start()
    {
        gameOverPanel.SetActive(false);

        KornnGameManager.Instance.OnGameEnded += ShowGameOver;

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f; // unpause

        // optional: reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnDestroy()
    {
        if (KornnGameManager.Instance != null)
        {
            KornnGameManager.Instance.OnGameEnded -= ShowGameOver;
        }
    }
}