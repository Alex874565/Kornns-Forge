using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerParticlesController : NetworkBehaviour
{
    [Serializable]
    private struct ParticleEntry
    {
        public MovementParticleType type;
        public ParticleSystemController particles;
        public Transform spawnPoint;
    }

    private enum MovementParticleType
    {
        Jump,
        Land,
        Sleep
    }

    [SerializeField] private ParticleEntry[] spawnableParticles;
    [SerializeField] private ParticleEntry[] existingParticles;

    private PlayerMovementController movement;
    private PlayerStatusController status;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        status = GetComponent<PlayerStatusController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        movement.OnInitiateJump += HandleJump;
        movement.OnLand += HandleLand;

        status.OnStartSleeping += HandleStartSleeping;
        status.OnStopSleeping += HandleStopSleeping;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        movement.OnInitiateJump -= HandleJump;
        movement.OnLand -= HandleLand;

        status.OnStartSleeping -= HandleStartSleeping;
        status.OnStopSleeping -= HandleStopSleeping;
    }

    private void HandleJump() => RequestSpawnParticles(MovementParticleType.Jump);
    private void HandleLand() => RequestSpawnParticles(MovementParticleType.Land);

    private void HandleStartSleeping() => RequestExistingParticles(MovementParticleType.Sleep, true);
    private void HandleStopSleeping() => RequestExistingParticles(MovementParticleType.Sleep, false);

    private void RequestSpawnParticles(MovementParticleType type)
    {
        ParticleEntry? entry = GetEntry(spawnableParticles, type);
        if (entry == null) return;

        Transform point = entry.Value.spawnPoint != null
            ? entry.Value.spawnPoint
            : transform;

        SpawnParticlesServerRpc(type, point.position);
    }

    private void RequestExistingParticles(MovementParticleType type, bool play)
    {
        ExistingParticlesServerRpc(type, play);
    }

    [ServerRpc]
    private void SpawnParticlesServerRpc(MovementParticleType type, Vector3 position)
    {
        SpawnParticlesClientRpc(type, position);
    }

    [ClientRpc]
    private void SpawnParticlesClientRpc(MovementParticleType type, Vector3 position)
    {
        ParticleEntry? entry = GetEntry(spawnableParticles, type);
        if (entry == null) return;

        ParticleSystemController particleSystemController;

        if (entry.Value.particles.parentToPlayer)
        {
            particleSystemController = Instantiate(
                entry.Value.particles,
                position,
                Quaternion.identity,
                transform
            );
        }
        else
        {
            particleSystemController = Instantiate(
                entry.Value.particles,
                position,
                Quaternion.identity
            );
        }

        particleSystemController.PlayParticles();
    }

    [ServerRpc]
    private void ExistingParticlesServerRpc(MovementParticleType type, bool play)
    {
        ExistingParticlesClientRpc(type, play);
    }

    [ClientRpc]
    private void ExistingParticlesClientRpc(MovementParticleType type, bool play)
    {
        ParticleEntry? entry = GetEntry(existingParticles, type);
        if (entry == null || entry.Value.particles == null) return;

        if (play)
            entry.Value.particles.PlayParticles();
        else
            entry.Value.particles.StopParticles();
    }

    private ParticleEntry? GetEntry(ParticleEntry[] entries, MovementParticleType type)
    {
        foreach (ParticleEntry entry in entries)
        {
            if (entry.type == type)
                return entry;
        }

        return null;
    }
}