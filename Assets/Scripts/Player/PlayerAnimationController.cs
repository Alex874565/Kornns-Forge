using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAnimationController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    private PlayerMovementController movement;
    private PlayerStatusController status;

    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int SleepingHash = Animator.StringToHash("isSleeping");

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
        status.OnStartSleeping += HandleStartSleeping;
        status.OnStopSleeping += HandleStopSleeping;
    }

    public override void OnNetworkDespawn()
    {
        movement.OnInitiateJump -= HandleInitiateJump;
        movement.OnLand -= HandleLand;
        movement.OnStartWalking -= HandleStartWalking;
        movement.OnEnterIdle -= HandleEnterIdle;
        status.OnStartSleeping -= HandleStartSleeping;
        status.OnStopSleeping -= HandleStopSleeping;
    }

    private void HandleInitiateJump()
    {
        if (!IsOwner) return;
        SetJumpServerRpc(true);
    }

    private void HandleLand()
    {
        if (!IsOwner) return;
        SetJumpServerRpc(false);
    }

    private void HandleStartWalking()
    {
        if (!IsOwner) return;
        SetMovingServerRpc(true);
    }

    private void HandleEnterIdle()
    {
        if (!IsOwner) return;
        SetMovingServerRpc(false);
    }

    private void HandleStartSleeping()
    {
        SetSleeping(true);
    }
    
    private void HandleStopSleeping()
    {
        SetSleeping(false);
    }

    [ServerRpc]
    private void SetMovingServerRpc(bool moving)
    {
        if (animator == null) return;
        animator.SetBool(MovingHash, moving);
    }

    [ServerRpc]
    private void SetJumpServerRpc(bool jumping)
    {
        if (animator == null) return;
        animator.SetBool(JumpHash, jumping);
    }
    
    private void SetSleeping(bool sleeping)
    {
        if (animator == null) return;

        animator.SetBool(SleepingHash, sleeping);
    }
}