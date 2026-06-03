using System;
using UnityEngine;

public class KornnGameManager : MonoBehaviour
{
    public static KornnGameManager Instance { get; private set; }

    [SerializeField] private float gameDuration = 120f; // seconds
    [SerializeField] private int levelNumber;

    private float remainingTime;
    private bool isGameRunning;
    

    public event Action<float> OnTimeChanged;
    public event Action OnGameEnded;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameManagers!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!isGameRunning) return;

        remainingTime -= Time.deltaTime;

        OnTimeChanged?.Invoke(remainingTime);

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isGameRunning = false;

            SaveStars();
            UnlockNextLevel();

            OnTimeChanged?.Invoke(remainingTime);
            OnGameEnded?.Invoke();

            Time.timeScale = 0f;

            Debug.Log("LEVEL COMPLETE");
        }
    }

    private void UnlockNextLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (levelNumber >= unlockedLevel)
        {
            int maxLevels = 3;

            PlayerPrefs.SetInt(
                "UnlockedLevel",
                Mathf.Min(levelNumber + 1, maxLevels)
            );

            PlayerPrefs.Save();
        }
    }

    public void StartGame()
    {
        remainingTime = gameDuration;
        isGameRunning = true;

        OnTimeChanged?.Invoke(remainingTime);
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public bool IsGameRunning()
    {
        return isGameRunning;
    }

    private void SaveStars()
    {
        int score = ScoreManager.Instance.GetScore();

        int stars = 0;

        if (score >= 100)
            stars = 1;

        if (score >= 200)
            stars = 2;

        if (score >= 300)
            stars = 3;


        string key = "Level" + levelNumber + "Stars";

        int previousStars = PlayerPrefs.GetInt(key, 0);

        // Only keep the best result
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }
}