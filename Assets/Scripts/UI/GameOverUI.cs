using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        Debug.Log("Restart Game was clicked");
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        NetworkAutoStarter.ShouldAutoStartHost = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (KornnGameManager.Instance != null)
        {
            KornnGameManager.Instance.OnGameEnded -= ShowGameOver;
        }
    }
}