using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Obstacle : MonoBehaviour
{
    public float speed= 10f;
    public float damage_energy=10;
    public Sprite sprite;
    public Transform position;
    public string poolTag;
    [Range(0,10)]public int weight;
    public float lifetime = 5f;

    private bool canFall = false;

    private void OnEnable()
    {
        StartCoroutine(DeactivateAfterTime());
    }

    public void ActivateFall(float delay)
    {
        Debug.Log($"ActivateFall called with delay: {delay}");
        StartCoroutine(FallRoutine(delay));
    }

    private IEnumerator FallRoutine(float delay)
    {
        canFall = false;
        Debug.Log("Waiting for fall delay...");
        yield return new WaitForSeconds(delay);
        canFall = true;
        Debug.Log("canFall is now true. Obstacle should start falling.");
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
            gameObject.SetActive(false); // Return to pool after hitting the player
            return;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }
}