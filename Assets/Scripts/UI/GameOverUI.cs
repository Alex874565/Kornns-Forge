using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button levelSelectButton;

    private void Start()
    {
        gameOverPanel.SetActive(false);

        KornnGameManager.Instance.OnGameEnded += ShowGameOver;

        restartButton?.onClick.AddListener(RestartGame);
        levelSelectButton?.onClick.AddListener(LoadLevelSelect);
    }

    private void LoadLevelSelect()
    {
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.SceneManager.LoadScene("LevelSelect", LoadSceneMode.Single);
        else
            SceneManager.LoadScene("LevelSelect");
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.OnGameEnded -= ShowGameOver;
    }
}