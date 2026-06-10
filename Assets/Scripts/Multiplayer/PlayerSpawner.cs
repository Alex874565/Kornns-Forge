using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Safety")]
    [SerializeField] private float spawnDelayAfterSceneLoad = 0.5f;

    private readonly HashSet<ulong> spawnedClients = new HashSet<ulong>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        StartCoroutine(DelayedSpawnAllConnectedClients());
    }

    private void OnLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!IsServer)
            return;

        StartCoroutine(DelayedSpawnAllConnectedClients());
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer)
            return;

        StartCoroutine(DelayedSpawnClient(clientId));
    }

    private IEnumerator DelayedSpawnAllConnectedClients()
    {
        yield return new WaitForSecondsRealtime(spawnDelayAfterSceneLoad);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerForClient(clientId);
        }
    }

    private IEnumerator DelayedSpawnClient(ulong clientId)
    {
        yield return new WaitForSecondsRealtime(spawnDelayAfterSceneLoad);

        SpawnPlayerForClient(clientId);
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (spawnedClients.Contains(clientId))
            return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        if (client.PlayerObject != null)
        {
            Debug.Log($"[PlayerSpawner] Client {clientId} already has PlayerObject. Skipping.");
            spawnedClients.Add(clientId);
            return;
        }

        Vector3 spawnPos = GetSpawnPosition(clientId);

        GameObject playerInstance = Instantiate(
            playerPrefab,
            spawnPos,
            Quaternion.identity
        );

        int playerIndex = spawnedClients.Count;

        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);

        PlayerAnimationController animationController =
            playerInstance.GetComponent<PlayerAnimationController>();

        animationController.SetPlayerIndex(playerIndex);

        spawnedClients.Add(clientId);

        Debug.Log($"[PlayerSpawner] Spawned player for client {clientId}");
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Vector3.zero;

        int index = spawnedClients.Count % spawnPoints.Length;
        return spawnPoints[index].position;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}