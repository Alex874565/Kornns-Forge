using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAnimationController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [SerializeField] private Transform visualTransform;

    private PlayerMovementController movement;
    private PlayerStatusController status;

    private static readonly int MovingHash = Animator.StringToHash("IsMoving");
    private static readonly int JumpHash = Animator.StringToHash("IsJumping");
    private static readonly int SleepingHash = Animator.StringToHash("IsSleeping");
    private static readonly int FallHash = Animator.StringToHash("IsFalling");

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        status = GetComponent<PlayerStatusController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        movement.OnInitiateJump += HandleInitiateJump;
        movement.OnLand += HandleLand;
        movement.OnStartWalking += HandleStartWalking;
        movement.OnEnterIdle += HandleEnterIdle;
        movement.OnJumpEnded += HandleJumpEnded;
        movement.OnStartFalling += HandleStartFalling;
        status.OnStartSleeping += HandleStartSleeping;
        status.OnStopSleeping += HandleStopSleeping;
    }

    public override void OnNetworkDespawn()
    {
        movement.OnInitiateJump -= HandleInitiateJump;
        movement.OnLand -= HandleLand;
        movement.OnStartWalking -= HandleStartWalking;
        movement.OnEnterIdle -= HandleEnterIdle;
        movement.OnJumpEnded -= HandleJumpEnded;
        movement.OnStartFalling -= HandleStartFalling;
        status.OnStartSleeping -= HandleStartSleeping;
        status.OnStopSleeping -= HandleStopSleeping;
    }

    private void HandleInitiateJump()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(JumpHash, true);
    }

    private void HandleJumpEnded()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(JumpHash, false);
    }

    private void HandleLand()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(FallHash, false);
    }

    private void HandleStartWalking()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(MovingHash, true);
    }

    private void HandleEnterIdle()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(MovingHash, false);
    }

    private void HandleStartSleeping()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(SleepingHash, true);
    }

    private void HandleStopSleeping()
    {
        if (!IsOwner) return;
        SetBoolServerRpc(SleepingHash, false);
    }

    private void HandleStartFalling()
    {
        if (!IsOwner) return;
        Debug.Log("Start Falling");
        SetBoolServerRpc(FallHash, true);
    }

    [ServerRpc]
    private void SetBoolServerRpc(int param, bool value)
    {
        SetBoolClientRpc(param, value);
    }

    [ClientRpc]
    private void SetBoolClientRpc(int param, bool value)
    {
        if (animator == null) return;
        animator.SetBool(param, value);
    }
}