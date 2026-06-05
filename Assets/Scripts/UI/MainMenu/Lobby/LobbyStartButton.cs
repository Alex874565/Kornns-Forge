using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyStartButton : MonoBehaviour
{
    [SerializeField] private Button startButton;

    [Header("Rules")]
    [SerializeField] private bool allowSoloStart = true;
    [SerializeField] private float waitAfterClientConnects = 1.5f;

    private float lastClientConnectedTime = -999f;

    private void Awake()
    {
        if (startButton == null)
            startButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
        {
            startButton.interactable = false;
            return;
        }

        int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (playerCount <= 0)
        {
            startButton.interactable = false;
            return;
        }

        if (!allowSoloStart && playerCount < 2)
        {
            startButton.interactable = false;
            return;
        }

        bool recentlyJoined =
            Time.unscaledTime - lastClientConnectedTime < waitAfterClientConnects;

        startButton.interactable = !recentlyJoined;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
            return;

        if (clientId == NetworkManager.Singleton.LocalClientId)
            return;

        lastClientConnectedTime = Time.unscaledTime;
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        lastClientConnectedTime = -999f;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }
}