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

        float movementVelocity = Ctx.Rb.linearVelocity.x;
        if (Ctx.Collision.CurrentMovingPlatform != null)
        {
            movementVelocity -= Ctx.Collision.CurrentMovingPlatform.HorizontalVelocity;
        }
        
        if (Mathf.Abs(movementVelocity) <= 0.1f)
            return MovementStateType.Idle;
        
        return null;
    }

    private void HandleGroundMovement()
    {
        if(Ctx.CanMove){
            Vector2 movement = Ctx.Input.Movement;

            if (Mathf.Abs(movement.x) > 0.1f)
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
        }
        else
        {
            Ctx.Velocity.x = 0f;
        }

        float platformVelocity = 0f;
        if(Ctx.Collision.CurrentMovingPlatform != null)
            platformVelocity = Ctx.Collision.CurrentMovingPlatform.HorizontalVelocity;

        Ctx.Velocity.y = 0f;
        
        Ctx.Rb.linearVelocity = new Vector2(Ctx.Velocity.x + platformVelocity, Ctx.Velocity.y);
    }
}