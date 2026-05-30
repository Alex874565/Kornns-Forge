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
        public ParticleSystemController prefab;
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

        ParticleSystemController particleSystemController;

        if (entry?.prefab.parentToPlayer == true)
        {
            particleSystemController = Instantiate(entry?.prefab, position, Quaternion.identity, gameObject.transform);
        }
        else
        {
            particleSystemController = Instantiate(entry?.prefab, position, Quaternion.identity);
        }
        particleSystemController.PlayParticles();
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
}