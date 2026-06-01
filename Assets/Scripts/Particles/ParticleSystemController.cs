using System.Collections;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    public bool parentToPlayer = false;
    public bool destroyAfterPlay = true;

    private ParticleSystem[] particles;
    private Coroutine playCoroutine;

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void PlayParticles()
    {
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);

        foreach (var particle in particles)
            particle.Play(true);

        if (destroyAfterPlay)
            playCoroutine = StartCoroutine(DestroyAfterPlayCoroutine());
    }

    private IEnumerator DestroyAfterPlayCoroutine()
    {
        yield return new WaitForSeconds(GetLifetime());
        Destroy(gameObject);
    }

    public void StopParticles()
    {
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);

        foreach (var particle in particles)
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public float GetLifetime()
    {
        float maxLifetime = 0f;

        foreach (var particle in particles)
        {
            var main = particle.main;
            float lifetime = main.duration + main.startLifetime.constantMax;
            maxLifetime = Mathf.Max(maxLifetime, lifetime);
        }

        return maxLifetime;
    }
}