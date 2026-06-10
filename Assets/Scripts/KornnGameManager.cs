using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class KornnGameManager : NetworkBehaviour
{
    public static KornnGameManager Instance { get; private set; }

    [SerializeField] private float gameDuration = 120f;
    [SerializeField] private int levelNumber;
    [SerializeField] private GameObject tutorial;
    public GameObject tutorialInitialButton;

    private PlayableDirector _ordersTimeline;

    private readonly NetworkVariable<float> remainingTime = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public readonly NetworkVariable<bool> IsPaused = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public readonly NetworkVariable<bool> TutorialOn = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public event Action<float> OnTimeChanged;
    public event Action OnGameEnded;

    private bool gameEndedInvoked;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameManagers!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _ordersTimeline = FindObjectOfType<PlayableDirector>();
    }

    public override void OnNetworkSpawn()
    {
        remainingTime.OnValueChanged += HandleTimeChanged;
        IsPaused.OnValueChanged += HandleGameRunningChanged;

        OnTimeChanged?.Invoke(remainingTime.Value);

        if (IsServer)
        {
            if (tutorial)
            {
                TutorialOn.Value = true;
                StartTutorialClientRpc();
            }
            else
            {
                StartGame();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        remainingTime.OnValueChanged -= HandleTimeChanged;
        IsPaused.OnValueChanged -= HandleGameRunningChanged;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (IsPaused.Value) return;

        remainingTime.Value -= Time.deltaTime;

        if (remainingTime.Value <= 0f)
        {
            remainingTime.Value = 0f;
            IsPaused.Value = true;

            SaveStars();
            UnlockNextLevel();

            EndGameClientRpc();

            Debug.Log("LEVEL COMPLETE");
        }
    }

    private void HandleTimeChanged(float oldValue, float newValue)
    {
        OnTimeChanged?.Invoke(newValue);
    }

    private void HandleGameRunningChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            gameEndedInvoked = false;
        
        Debug.Log(newValue ? "ON" : "OFF");
        if(newValue)
            _ordersTimeline.Resume();
        else
            _ordersTimeline.Pause();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void FinishTutorialServerRpc()
    {
        TutorialOn.Value = false;
        StartGame();
        FinishTutorialClientRpc();
    }

    [ClientRpc]
    private void FinishTutorialClientRpc()
    {
        PlayerInputController playerInputController =
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerInputController>();

        playerInputController.SetUIMode(false);

        if (tutorial != null)
            tutorial.SetActive(false);
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        if (gameEndedInvoked) return;

        gameEndedInvoked = true;
        OnTimeChanged?.Invoke(remainingTime.Value);
        OnGameEnded?.Invoke();
    }

    public void StartGame()
    {
        if (!IsServer) return;

        gameEndedInvoked = false;
        remainingTime.Value = gameDuration;
        IsPaused.Value = false;
        _ordersTimeline.Play();
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

    private void SaveStars()
    {
        int score = ScoreManager.Instance.GetScore();

        int stars = 0;

        if (score >= 100) stars = 1;
        if (score >= 200) stars = 2;
        if (score >= 300) stars = 3;

        string key = "Level" + levelNumber + "Stars";

        int previousStars = PlayerPrefs.GetInt(key, 0);

        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }
    
    [ClientRpc]
    private void StartTutorialClientRpc()
    {
        if (tutorial != null)
        {
            tutorial.SetActive(true);
        }
    }
}