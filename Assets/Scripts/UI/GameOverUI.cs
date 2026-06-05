using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button levelSelectButton;

    [Header("Stars")]
    [SerializeField] private Image[] stars;
    [SerializeField] private int oneStarScore = 100;
    [SerializeField] private int twoStarScore = 250;
    [SerializeField] private int threeStarScore = 500;

    [SerializeField] private Color unlockedStarColor = Color.white;
    [SerializeField] private Color lockedStarColor = new Color(1f, 1f, 1f, 0.25f);

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.OnGameEnded += ShowGameOver;

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (levelSelectButton != null)
            levelSelectButton.onClick.AddListener(LoadLevelSelect);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateStars();
    }

    private void UpdateStars()
    {
        int score = GetScore();
        int starCount = 0;

        if (score >= oneStarScore)
            starCount = 1;

        if (score >= twoStarScore)
            starCount = 2;

        if (score >= threeStarScore)
            starCount = 3;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            stars[i].color = i < starCount
                ? unlockedStarColor
                : lockedStarColor;
        }
    }

    private int GetScore()
    {
        if (ScoreManager.Instance == null)
            return 0;

        return ScoreManager.Instance.GetScore();
    }

    private void RestartGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            RequestRestartServerRpc();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void LoadLevelSelect()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            RequestLevelSelectServerRpc();
        }
        else
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRestartServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            SceneManager.GetActiveScene().name,
            LoadSceneMode.Single
        );
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestLevelSelectServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            "LevelSelect",
            LoadSceneMode.Single
        );
    }

    private void OnDestroy()
    {
        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.OnGameEnded -= ShowGameOver;

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (levelSelectButton != null)
            levelSelectButton.onClick.RemoveListener(LoadLevelSelect);
    }
}