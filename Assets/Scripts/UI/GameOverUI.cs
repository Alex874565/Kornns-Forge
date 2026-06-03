using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
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
    [SerializeField] private Color lockedStarColor = new Color(1, 1, 1, 0.25f);


    private void Start()
    {
        gameOverPanel.SetActive(false);

        KornnGameManager.Instance.OnGameEnded += ShowGameOver;

        restartButton?.onClick.AddListener(RestartGame);
        levelSelectButton?.onClick.AddListener(LoadLevelSelect);
    }


    private void ShowGameOver()
    {
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
            if (i < starCount)
                stars[i].color = unlockedStarColor;
            else
                stars[i].color = lockedStarColor;
        }
    }


    private int GetScore()
    {
        return ScoreManager.Instance.GetScore();
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
            NetworkManager.Singleton.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name,
                LoadSceneMode.Single
            );
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private void OnDestroy()
    {
        if (KornnGameManager.Instance != null)
            KornnGameManager.Instance.OnGameEnded -= ShowGameOver;
    }
}