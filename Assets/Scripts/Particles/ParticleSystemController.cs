using System.Collections;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    public bool parentToPlayer = false;
    
    private ParticleSystem[] particles;
    private float duration;
    
    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            if(particle.main.duration > duration)
                duration = particle.main.duration;
        }
    }

    public void PlayParticles()
    {
        StartCoroutine(PlayParticlesCoroutine());
    }

    private IEnumerator PlayParticlesCoroutine()
    {
        foreach (var particle in particles)
            particle.Play();

        yield return new WaitForSeconds(duration);
        
        Destroy(gameObject); 
    }
    
    #region Test

    [ContextMenu("PlayParticles")]
    private void PlayParticlesTest()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            particle.Play();
        }
    }
    
    #endregion
}