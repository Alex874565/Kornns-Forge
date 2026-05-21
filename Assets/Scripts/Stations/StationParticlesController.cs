using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class StationParticlesController : MonoBehaviour
{
    [SerializeField] private BaseStation station;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem processingParticles;
    [FormerlySerializedAs("interactionParticles")] [SerializeField] private ParticleSystem interactParticles;
    [SerializeField] private ParticleSystem interactAlternateParticles;

    private void OnEnable()
    {
        station.OnInteract += OnInteract;
        station.OnStartProcessing += OnStartProcessing;
        station.OnStopProcessing += OnStopProcessing;
        station.OnInteractAlternate += OnInteractAlternate;
    }

    private void OnDisable()
    {
        station.OnInteract -= OnInteract;
        station.OnStartProcessing -= OnStartProcessing;
        station.OnStopProcessing -= OnStopProcessing;
        station.OnInteractAlternate -= OnInteractAlternate;
    }

    private void OnInteract()
    {
        if(interactParticles)
            interactParticles.Play();
    }
    
    private void OnInteractAlternate()
    {
        Debug.Log("Alternate interact triggered");
        if(interactAlternateParticles)
            interactAlternateParticles.Play();
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