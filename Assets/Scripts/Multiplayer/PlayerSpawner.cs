using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Listen for when clients finish loading the scene
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        
        // Also check if we should spawn for the host immediately if the scene is already loaded
        // However, OnLoadEventCompleted usually covers the host too.
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        Debug.Log($"[PlayerSpawner] Scene load completed for {clientsCompleted.Count} clients. Starting manual spawn.");

        foreach (var clientId in clientsCompleted)
        {
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        // Check if player already exists for this client to avoid duplicates
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                Debug.Log($"[PlayerSpawner] Client {clientId} already has a player object. Skipping.");
                return;
            }

            Vector3 spawnPos = Vector3.zero;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPos = spawnPoints[(int)clientId % spawnPoints.Length].position;
            }

            GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            
            // This is the manual spawn command that marks this object as the player's main avatar
            networkObject.SpawnAsPlayerObject(clientId, true);
            
            Debug.Log($"[PlayerSpawner] Manually spawned player for Client {clientId} at {spawnPos}");
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }
        base.OnDestroy();
    }
}

