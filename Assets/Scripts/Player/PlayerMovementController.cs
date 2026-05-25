﻿using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PlayerMovementContext), typeof(PlayerCollisionController), typeof(PlayerInteractionController))]
public class PlayerMovementController : NetworkBehaviour
{
    [Header("References")]
    [field: SerializeField] public PlayerMovementStats MovementStats { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }

    public event Action OnRotation;
    public event Action OnInitiateJump;
    public event Action OnLand;
    public event Action OnBumpHead;
    public event Action OnStartFalling;
    public event Action OnStartWalking;
    public event Action OnEnterIdle;

    private PlayerMovementContext _ctx;
    private MovementFSM _fsm;

    private bool _isFacingRight;

    public bool IsInteracting { get; set; }

    public override void OnNetworkSpawn()
    {
        _isFacingRight = true;
        _ctx = new PlayerMovementContext
        {
            Rb = GetComponent<Rigidbody2D>(),
            Input = GetComponent<PlayerInputController>(),
            Collision = GetComponent<PlayerCollisionController>(),
            Interaction = GetComponent<PlayerInteractionController>(),
            Stats = MovementStats,
            Velocity = Vector2.zero,
            CanMove = true
        };

        _fsm = new MovementFSM(_ctx, this);

        _ctx.Input.enabled = IsOwner;

        if (Animator == null)
            Animator = GetComponentInChildren<Animator>();

        if (Animator == null)
            Debug.LogWarning("PlayerMovementController: Animator not assigned or found on children — jump animation won't play.");

        _ctx.Input.OnMove += TryToRotate;
        _ctx.Input.OnJumpPressed += HandleJumpPressed;
        _ctx.Input.OnJumpReleased += HandleJumpReleased;
    }

    private void OnDestroy()
    {
        _ctx.Input.OnMove -= TryToRotate;
        _ctx.Input.OnJumpPressed -= HandleJumpPressed;
        _ctx.Input.OnJumpReleased -= HandleJumpReleased;
    }

    private void Update()
    {
        if (!IsOwner) return;

        UpdateTimers();

        _fsm?.Update();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        _fsm?.FixedUpdate();
    }

    public void StopMovement()
    {
        _ctx.CanMove = false;
    }

    public void StartMovement()
    {
        _ctx.CanMove = true;
    }

    public void MoveTo(Vector2 position)
    {
        _ctx.Rb.MovePosition(position);
    }

    private void UpdateTimers()
    {
        if (_ctx.JumpBufferTimer > 0f)
        {
            _ctx.JumpBufferTimer -= Time.deltaTime;
            if (_ctx.JumpBufferTimer < 0f)
                _ctx.JumpBufferTimer = 0f;
        }

        if (_ctx.JumpCoyoteTimer > 0f)
        {
            _ctx.JumpCoyoteTimer -= Time.deltaTime;
            if (_ctx.JumpCoyoteTimer < 0f)
                _ctx.JumpCoyoteTimer = 0f;
        }

        if (_ctx.Collision.IsGrounded)
        {
            _ctx.JumpCoyoteTimer = _ctx.Stats.JumpCoyoteTime;
        }
    }

    private void HandleJumpPressed()
    {
        _ctx.JumpBufferTimer = _ctx.Stats.JumpBufferTime;
        _ctx.JumpReleasedDuringBuffer = false;
    }

    private void HandleJumpReleased()
    {
        if (_ctx.JumpBufferTimer > 0f)
            _ctx.JumpReleasedDuringBuffer = true;

        _ctx.JumpReleased = true;
    }

    private void TryToRotate(Vector2 movement)
    {
        if (_isFacingRight && movement.x < 0f)
            Rotate(false);
        else if (!_isFacingRight && movement.x > 0f)
            Rotate(true);
    }

    private void Rotate(bool faceRight)
    {
        _isFacingRight = faceRight;

        transform.rotation = faceRight
            ? Quaternion.Euler(0f, 0f, 0f)
            : Quaternion.Euler(0f, 180f, 0f);

        OnRotation?.Invoke();
    }

    public void InvokeOnInitiateJump()
    {
        if (Animator != null)
            Animator.SetBool("Jump", true);

        OnInitiateJump?.Invoke();
    }

    public void InvokeOnLand()
    {
        if (Animator != null)
            Animator.SetBool("Jump", false);

        OnLand?.Invoke();
    }

    public void InvokeOnBumpHead()
    {
        OnBumpHead?.Invoke();
    }
    public void InvokeOnStartFalling() => OnStartFalling?.Invoke();

    public void InvokeOnStartWalking()
    {
        if (Animator != null)
            Animator.SetBool("Moving", true);

        OnStartWalking?.Invoke();
    }

    public void InvokeOnEnterIdle()
    {
        if (Animator != null)
            Animator.SetBool("Moving", false);

        OnEnterIdle?.Invoke();
    }
}