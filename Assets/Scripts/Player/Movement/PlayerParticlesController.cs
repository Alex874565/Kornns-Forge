using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerParticlesController : NetworkBehaviour
{
    [Serializable]
    private struct ParticleEntry
    {
        public MovementParticleType type;
        public ParticleSystem prefab;
        public Transform spawnPoint;
    }

    private enum MovementParticleType
    {
        Jump,
        Land,
        Dash,
        Death
    }

    [SerializeField] private ParticleEntry[] particles;

    private PlayerMovementController movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        movement.OnInitiateJump += HandleJump;
        movement.OnLand += HandleLand;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        movement.OnInitiateJump -= HandleJump;
        movement.OnLand -= HandleLand;
    }

    private void HandleJump() => RequestParticles(MovementParticleType.Jump);
    private void HandleLand() => RequestParticles(MovementParticleType.Land);

    private void RequestParticles(MovementParticleType type)
    {
        ParticleEntry? entry = GetEntry(type);
        if (entry == null) return;

        Transform point = entry.Value.spawnPoint != null
            ? entry.Value.spawnPoint
            : transform;

        PlayParticlesServerRpc(type, point.position);
    }

    [ServerRpc]
    private void PlayParticlesServerRpc(MovementParticleType type, Vector3 position)
    {
        PlayParticlesClientRpc(type, position);
    }

    [ClientRpc]
    private void PlayParticlesClientRpc(MovementParticleType type, Vector3 position)
    {
        ParticleEntry? entry = GetEntry(type);
        if (entry == null) return;

        ParticleSystem instance = Instantiate(entry?.prefab, position, Quaternion.identity);
        StartCoroutine(DestroyAfterPlaying(instance));
    }

    private ParticleEntry? GetEntry(MovementParticleType type)
    {
        foreach (ParticleEntry entry in particles)
        {
            if (entry.type == type)
                return entry;
        }

        return null;
    }

    private IEnumerator DestroyAfterPlaying(ParticleSystem particles)
    {
        particles.Play();

        float lifetime = particles.main.duration +
                         particles.main.startLifetime.constantMax;

        yield return new WaitForSeconds(lifetime);

        Destroy(particles.gameObject);
    }
}