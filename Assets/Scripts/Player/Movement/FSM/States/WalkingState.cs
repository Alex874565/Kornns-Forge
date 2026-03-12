using UnityEngine;
using System;

public class WalkingState : MovementState
{
    private readonly Action _onStartWalking;
    
    public WalkingState(PlayerMovementContext ctx, Action onStartWalking) : base(ctx)
    {
        _onStartWalking = onStartWalking;
    }

    public override void Enter()
    {
        Ctx.IsFastFalling = false;
        Ctx.FastFallTime = 0f;
        Ctx.IsPastApexThreshold = false;
        Ctx.TimePastApexThreshold = 0f;

        Ctx.Velocity.y = 0f;

        _onStartWalking?.Invoke();
    }

    public override void FixedUpdate()
    {
        HandleGroundMovement();
        ApplyVelocity();
    }

    public override MovementStateType? NextState()
    {
        if (Ctx.JumpBufferTimer > 0f &&
            (Ctx.Collision.IsGrounded || Ctx.JumpCoyoteTimer > 0f))
        {
            return MovementStateType.Jumping;
        }

        if (!Ctx.Collision.IsGrounded)
            return MovementStateType.Falling;

        if (Mathf.Abs(Ctx.Input.Movement.x) > 0.01f)
            return MovementStateType.Walking;

        return null;
    }

    private void HandleGroundMovement()
    {
        Vector2 movement = Ctx.Input.Movement;

        if (movement != Vector2.zero)
        {
            float targetVelocityX = movement.x * Ctx.Stats.MaxSpeed;
            Ctx.Velocity.x = Mathf.Lerp(
                Ctx.Velocity.x,
                targetVelocityX,
                Ctx.Stats.Acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            Ctx.Velocity.x = Mathf.Lerp(
                Ctx.Velocity.x,
                0f,
                Ctx.Stats.Deceleration * Time.fixedDeltaTime
            );
        }

        Ctx.Velocity.y = 0f;
    }

    private void ApplyVelocity()
    {
        Ctx.Rb.linearVelocity = Ctx.Velocity;
    }
}