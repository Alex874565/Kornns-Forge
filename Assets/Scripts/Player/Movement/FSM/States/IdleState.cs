using UnityEngine;
using System;

public class IdleState : MovementState
{
    private readonly Action _onEnterIdle;
    
    public IdleState(PlayerMovementContext ctx, Action onEnterIdle) : base(ctx)
    {
        _onEnterIdle = onEnterIdle;
    }

    public override void Enter()
    {
        Ctx.IsFastFalling = false;
        Ctx.FastFallTime = 0f;
        Ctx.IsPastApexThreshold = false;
        Ctx.TimePastApexThreshold = 0f;

        _onEnterIdle?.Invoke();
        Ctx.Velocity.y = 0f;

    }

    public override void FixedUpdate()
    {
        Ctx.Velocity.x = Mathf.Lerp(
            Ctx.Velocity.x,
            0f,
            Ctx.Stats.Deceleration * Time.fixedDeltaTime
        );

        Ctx.Velocity.y = 0f;
        Ctx.Rb.linearVelocity = Ctx.Velocity;
    }

    public override MovementStateType? NextState()
    {
        if(Ctx.Interaction.IsInteracting)
            return MovementStateType.Interacting;
        
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
}