using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Obstacle : MonoBehaviour
{
    public float speed = 10f;
    public float damage_energy = 10;
    public Sprite sprite;
    public Transform position;
    public string poolTag;
    [Range(0, 10)] public int weight;
    public float lifetime = 5f;
    public float stunDuration = 2f;
    public float spawnDelayOnHit = 1.5f;

    private bool canFall = false;

    private void OnEnable()
    {
        StartCoroutine(DeactivateAfterTime());
    }

    public void ActivateFall(float delay)
    {
        StartCoroutine(FallRoutine(delay));
    }

    private IEnumerator FallRoutine(float delay)
    {
        canFall = false;
        yield return new WaitForSeconds(delay);
        canFall = true;
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (canFall)
        {
            transform.Translate(Vector2.down * speed * Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        canFall = false;
        StopAllCoroutines();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the obstacle collided with the player
        if (collision.gameObject.TryGetComponent<PlayerStatusController>(out PlayerStatusController player))
        {
            player.GetTired(damage_energy);
            // Request stun on the player's owning client (server will forward the RPC)
            if (player.IsServer)
            {
                // If this code runs on the server, directly call the server RPC
                player.RequestStunServerRpc(stunDuration);
            }
            else
            {
                // If not on server, attempt to call server RPC - will only take effect on server-side processing
                player.RequestStunServerRpc(stunDuration);
            }

            // Increase time before next spawn globally (if manager present)
            if (ObstacleManager.Instance != null)
            {
                ObstacleManager.Instance.AddSpawnDelay(spawnDelayOnHit);
            }
            gameObject.SetActive(false); // Return to pool after hitting the player
            return;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }
}