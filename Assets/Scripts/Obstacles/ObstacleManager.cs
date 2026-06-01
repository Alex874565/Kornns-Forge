using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ObstacleManager : NetworkBehaviour
{
    public static ObstacleManager Instance { get; private set; }

    // Additional spawn delay to add to the next spawn (seconds)
    private float spawnCooldownTimer = 0f;

    [System.Serializable]
    public struct ObstacleEntry
    {
        [Tooltip("This name must match the tag used in the ObjectPooler.")]
        public string poolTag;
        [Tooltip("How likely this obstacle is to be chosen compared to others.")]
        [Range(0, 10)] public int weight;
    }

    [Header("Spawning Configuration")]
    [SerializeField] private List<ObstacleEntry> obstacles;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 1.5f;
    [Tooltip("Multiplier applied to the randomly chosen spawn interval (use >1 to slow spawns)")]
    [SerializeField] private float spawnIntervalMultiplier = 1.5f;
    [SerializeField] private float fallDelay = 3f;

    [Header("Spawn Area")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Warning Settings")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 0.5f;

    private int totalWeight;

    public override void OnNetworkSpawn()
    {
        Instance = this;

        if (!IsServer)
        {
            enabled = false;
            return;
        }

        if (obstacles == null || obstacles.Count == 0)
        {
            Debug.LogError("Obstacles list is not configured in ObstacleManager.", this);
            enabled = false;
            return;
        }

        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area is not configured in ObstacleManager.", this);
            enabled = false;
            return;
        }

        totalWeight = obstacles.Sum(o => o.weight);
        
        if (IsServer)
        {
            StartCoroutine(SpawnObstacleRoutine());
        }
    }

    private IEnumerator SpawnObstacleRoutine()
    {
        while (true)
        {
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval) * spawnIntervalMultiplier;
            if (spawnCooldownTimer > 0f)
            {
                spawnInterval += spawnCooldownTimer;
                spawnCooldownTimer = 0f;
            }
            yield return new WaitForSeconds(spawnInterval);

            SpawnRandomObstacleFromPool();
        }
    }

    // Allow external code to set a multiplier at runtime (optional)
    public void SetSpawnIntervalMultiplier(float multiplier)
    {
        spawnIntervalMultiplier = Mathf.Max(0.01f, multiplier);
    }

    private void SpawnRandomObstacleFromPool()
    {
        string obstacleTag = GetRandomObstacleTag();
        if (string.IsNullOrEmpty(obstacleTag)) return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        SpawnObstacleClientRpc(obstacleTag, spawnPosition);
    }

    [ClientRpc]
    private void SpawnObstacleClientRpc(string obstacleTag, Vector3 spawnPosition)
    {
        StartCoroutine(ShowWarningAndSpawn(obstacleTag, spawnPosition));
    }

    private IEnumerator ShowWarningAndSpawn(string obstacleTag, Vector3 spawnPosition)
    {
        GameObject warningInstance = null;
        if (warningPrefab != null)
        {
            // Place the warning at the bottom of the spawn area (opposite the top spawn)
            Vector3 warningPosition = new Vector3(spawnPosition.x, -8, spawnPosition.z);
            // Slight upward offset so the warning is visible above the very bottom edge
            warningPosition += Vector3.up * 0.5f;
            warningInstance = Instantiate(warningPrefab, warningPosition, Quaternion.identity);
        }

        yield return new WaitForSeconds(warningDuration);

        if (warningInstance != null)
            Destroy(warningInstance);

        GameObject obstacleInstance = ObjectPooler.Instance.SpawnFromPool(obstacleTag, spawnPosition, fallDelay);
        if (obstacleInstance == null)
            Debug.LogWarning($"ObjectPooler failed to spawn an obstacle with tag '{obstacleTag}'.");
    }

    // Add extra delay (in seconds) to the next spawn cycle. Server-only.
    public void AddSpawnDelay(float seconds)
    {
        if (!IsServer) return;
        spawnCooldownTimer += seconds;
    }

    private string GetRandomObstacleTag()
    {
        if (totalWeight <= 0) return null;
        int randomWeight = Random.Range(0, totalWeight);
        foreach (var entry in obstacles)
        {
            if (randomWeight < entry.weight)
            {
                return entry.poolTag;
            }
            randomWeight -= entry.weight;
        }

        // Fallback in case of rounding issues
        return obstacles.LastOrDefault().poolTag;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnArea.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        // Spawn at the top of the spawn area
        float y = bounds.max.y;
        return new Vector3(x, y, 0);
    }
}