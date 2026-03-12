using UnityEngine;

public class PlayerMovementContext
{
    public Rigidbody2D Rb;
    public PlayerInputController Input;
    public PlayerCollisionController Collision;
    public PlayerMovementStats Stats;
    public PlayerInteractionController Interaction;

    public Vector2 Velocity;

    public bool JumpReleased;
    
    public bool IsFastFalling;
    public float FastFallTime;
    public float FastFallReleaseSpeed;

    public bool IsPastApexThreshold;
    public float TimePastApexThreshold;

    public bool JumpReleasedDuringBuffer;
    public float JumpBufferTimer;

    public float JumpCoyoteTimer;
}