using System;
using UnityEngine;

public class KornnGameManager : MonoBehaviour
{
    public static KornnGameManager Instance { get; private set; }

    [SerializeField] private float gameDuration = 120f; // seconds

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

            OnTimeChanged?.Invoke(remainingTime);
            OnGameEnded?.Invoke();

            Time.timeScale = 0f; // 🔥 pauses the game

            Debug.Log("GAME OVER");
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
}