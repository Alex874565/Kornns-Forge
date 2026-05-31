using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MultiplayerGameManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Level2";

    [Header("UI References")]
    [SerializeField] private Button startGameButton;

    private void Awake()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            // Make sure the manager persists
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            
            // Listen for connections to handle potential late-join spawning if needed
            // but primarily we rely on auto-spawn for now.
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null || startGameButton == null) return;

        bool isHost = NetworkManager.Singleton.IsServer;
        bool isListening = NetworkManager.Singleton.IsListening;

        if (startGameButton.gameObject.activeSelf != (isHost && isListening))
        {
            startGameButton.gameObject.SetActive(isHost && isListening);
        }

        // Host can start even if alone (1 or 2 players)
        startGameButton.interactable = NetworkManager.Singleton.ConnectedClientsList.Count >= 1;
    }

    private void OnStartGameClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"[MultiplayerGameManager] Loading game scene: {gameSceneName}");
            
            // Set a flag or state if needed, but NGO's SceneManager handles the sync
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
}


