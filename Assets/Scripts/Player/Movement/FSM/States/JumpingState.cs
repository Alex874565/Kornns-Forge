using UnityEngine;
using System;

public class JumpingState : MovementState
{
    private readonly Action _onStartJumping;
    private readonly Action _onBumpedHead;

    public JumpingState(PlayerMovementContext ctx, Action onStartJumping, Action onBumpedHead) : base(ctx)
    {
        _onStartJumping = onStartJumping;
        _onBumpedHead = onBumpedHead;
    }

    public override void Enter()
    {
        Ctx.JumpBufferTimer = 0f;
        Ctx.Velocity.y = Ctx.Stats.InitialJumpVelocity;

        Ctx.IsFastFalling = false;
        Ctx.FastFallTime = 0f;
        Ctx.IsPastApexThreshold = false;
        Ctx.TimePastApexThreshold = 0f;

        if (Ctx.JumpReleasedDuringBuffer)
        {
            Ctx.IsFastFalling = true;
            Ctx.FastFallTime = 0f;
            Ctx.FastFallReleaseSpeed = Ctx.Velocity.y;
        }

        Ctx.JumpReleasedDuringBuffer = false;
        Ctx.JumpReleased = false;

        _onStartJumping?.Invoke();
    }

    public override void FixedUpdate()
    {
        HandleAirMovement();
        
        if (Ctx.Collision.BumpedHead && Ctx.Velocity.y > 0f)
        {
            Ctx.IsFastFalling = true;
            Ctx.FastFallTime = Ctx.Stats.TimeForUpwardsCancel;
            Ctx.FastFallReleaseSpeed = 0f;
            Ctx.Velocity.y = -0.01f;
            _onBumpedHead?.Invoke();
        }

        if (Ctx.JumpReleased && Ctx.Velocity.y > 0f && !Ctx.IsFastFalling)
        {
            StartJumpCut();
            Ctx.JumpReleased = false;
        }

        if (Ctx.IsFastFalling)
        {
            HandleJumpCut();
            ApplyVelocity();
            return;
        }

        if (Ctx.Velocity.y >= 0f)
            HandleRising();
        else
            ApplyFallingGravity();

        ApplyVelocity();
    }

    public override void Exit()
    {
        Ctx.IsPastApexThreshold = false;
        Ctx.TimePastApexThreshold = 0f;
        Ctx.JumpReleased = false;
    }

    public override MovementStateType? NextState()
    {
        if (Ctx.Collision.IsGrounded && Ctx.Velocity.y <= 0f)
        {
            if (Mathf.Abs(Ctx.Input.Movement.x) > 0.01f)
                return MovementStateType.Walking;

            return MovementStateType.Idle;
        }

        if (Ctx.Velocity.y < 0f)
            return MovementStateType.Falling;

        return null;
    }

    private void StartJumpCut()
    {
        if (Ctx.IsPastApexThreshold)
        {
            Ctx.IsPastApexThreshold = false;
            Ctx.IsFastFalling = true;
            Ctx.FastFallTime = Ctx.Stats.TimeForUpwardsCancel;
            Ctx.Velocity.y = 0f;
        }
        else
        {
            Ctx.IsFastFalling = true;
            Ctx.FastFallTime = 0f;
            Ctx.FastFallReleaseSpeed = Ctx.Velocity.y;
        }
    }

    private void HandleRising()
    {
        float apexPoint = Mathf.InverseLerp(
            Ctx.Stats.InitialJumpVelocity,
            0f,
            Ctx.Velocity.y
        );

        if (apexPoint > Ctx.Stats.ApexThreshold)
        {
            if (!Ctx.IsPastApexThreshold)
            {
                Ctx.IsPastApexThreshold = true;
                Ctx.TimePastApexThreshold = 0f;
            }

            Ctx.TimePastApexThreshold += Time.fixedDeltaTime;

            if (Ctx.TimePastApexThreshold < Ctx.Stats.ApexHangTime)
                Ctx.Velocity.y = 0f;
            else
                Ctx.Velocity.y = -0.01f;
        }
        else
        {
            Ctx.Velocity.y += Ctx.Stats.Gravity * Time.fixedDeltaTime;
        }
    }

    private void HandleJumpCut()
    {
        if (Ctx.FastFallTime >= Ctx.Stats.TimeForUpwardsCancel)
        {
            Ctx.Velocity.y += Ctx.Stats.Gravity *
                              Ctx.Stats.GravityReleaseMultiplier *
                              Time.fixedDeltaTime;
        }
        else
        {
            Ctx.Velocity.y = Mathf.Lerp(
                Ctx.FastFallReleaseSpeed,
                0f,
                Ctx.FastFallTime / Ctx.Stats.TimeForUpwardsCancel
            );
        }

        Ctx.FastFallTime += Time.fixedDeltaTime;
    }

    private void ApplyFallingGravity()
    {
        Ctx.Velocity.y += Ctx.Stats.Gravity *
                          Ctx.Stats.GravityReleaseMultiplier *
                          Time.fixedDeltaTime;
    }

    private void ApplyVelocity()
    {
        Ctx.Velocity.y = Mathf.Clamp(Ctx.Velocity.y, -Ctx.Stats.MaxFallSpeed, 50f);
        Ctx.Rb.linearVelocity = Ctx.Velocity;
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