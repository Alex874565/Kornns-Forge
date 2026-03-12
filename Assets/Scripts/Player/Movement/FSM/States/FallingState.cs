using UnityEngine;
using System;

public class FallingState : MovementState
{
    private readonly Action _onStartFalling, _onLand;
    
    public FallingState(PlayerMovementContext ctx, Action onStartFalling, Action onLand) : base(ctx)
    {
        _onStartFalling = onStartFalling;
        _onLand = onLand;
    }

    public override void Enter()
    {
        _onStartFalling?.Invoke();
    }

    public override void FixedUpdate()
    {
        HandleAirMovement();
        HandleFall();
    }

    public override void Exit()
    {
        Ctx.IsFastFalling = false;
        Ctx.FastFallTime = 0f;
        Ctx.IsPastApexThreshold = false;
        
        _onLand?.Invoke();
    }

    public override MovementStateType? NextState()
    {
        if (Ctx.Collision.IsGrounded)
        {
            if (Mathf.Abs(Ctx.Input.Movement.x) > 0.01f)
                return MovementStateType.Walking;

            return MovementStateType.Idle;
        }

        if (Ctx.JumpBufferTimer > 0f && Ctx.JumpCoyoteTimer > 0f)
            return MovementStateType.Jumping;

        return null;
    }

    private void HandleFall()
    {
        if (Ctx.IsFastFalling)
        {
            HandleFastFall();
        }
        else
        {
            Ctx.Velocity.y += Ctx.Stats.Gravity * Time.fixedDeltaTime;
        }

        Ctx.Velocity.y = Mathf.Clamp(Ctx.Velocity.y, -Ctx.Stats.MaxFallSpeed, 50f);
        Ctx.Rb.linearVelocity = Ctx.Velocity;
    }

    private void HandleFastFall()
    {
        if (Ctx.FastFallTime < Ctx.Stats.TimeForUpwardsCancel && Ctx.FastFallReleaseSpeed > 0f)
        {
            Ctx.Velocity.y = Mathf.Lerp(
                Ctx.FastFallReleaseSpeed,
                0f,
                Ctx.FastFallTime / Ctx.Stats.TimeForUpwardsCancel
            );
        }
        else
        {
            Ctx.Velocity.y += Ctx.Stats.Gravity * Ctx.Stats.GravityReleaseMultiplier * Time.fixedDeltaTime;
        }

        Ctx.FastFallTime += Time.fixedDeltaTime;
    }
    
    private void HandleAirMovement()
    {
        Vector2 movement = Ctx.Input.Movement;

        if (movement != Vector2.zero)
        {
            float targetVelocityX = movement.x * Ctx.Stats.MaxSpeed;
            Ctx.Velocity.x = Mathf.Lerp(
                Ctx.Velocity.x,
                targetVelocityX,
                Ctx.Stats.AirAcceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            Ctx.Velocity.x = Mathf.Lerp(
                Ctx.Velocity.x,
                0f,
                Ctx.Stats.AirDeceleration * Time.fixedDeltaTime
            );
        }
    }
}