using Unity.Netcode;
using UnityEngine;

public class StationParticlesController : MonoBehaviour
{
    [SerializeField] private BaseStation station;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem processingParticles;
    [SerializeField] private ParticleSystem interactionParticles;

    private void OnEnable()
    {
        station.OnInteract += OnInteract;
        station.OnStartProcessing += OnStartProcessing;
        station.OnStopProcessing += OnStopProcessing;
    }

    private void OnDisable()
    {
        station.OnInteract -= OnInteract;
        station.OnStartProcessing -= OnStartProcessing;
        station.OnStopProcessing -= OnStopProcessing;
    }

    private void OnInteract()
    {
        if(interactionParticles)
            interactionParticles.Play();
    }
    
    private void OnStartProcessing()
    {
        if(processingParticles)
            processingParticles.Play();
    }

    private void OnStopProcessing()
    {
        if(processingParticles)
            processingParticles.Stop();
    }
}