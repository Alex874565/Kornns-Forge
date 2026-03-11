using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Player/MovementStats", fileName = "MovementStats")]
public class PlayerMovementStats : ScriptableObject
{
    [field: Header("Ground Movement")]
    [field: SerializeField] 
    [field: Range(1f, 100f)] 
    public float MaxSpeed { get; private set; } = 12.5f;
    [field: SerializeField]
    [field: Range(.25f, 50f)]
    public float Acceleration { get; private set; } = 5f;
    [field: SerializeField]
    [field: Range(.25f, 50f)]
    public float Deceleration { get; private set; } = 20f;

    [field: Header("Air Movement")]
    [field: SerializeField]
    [field: Range(.25f, 50f)]
    public float AirAcceleration { get; private set; } = 5f;
    [field: SerializeField]
    [field: Range(.25f, 50f)]
    public float AirDeceleration { get; private set; } = 5f;
    
    [field: Header("Jumping")]
    [field: SerializeField] public float JumpHeight { get; private set; }
    [field: SerializeField] public float JumpHeightCompensationFactor { get; private set; } = 1.025f;
    [field: SerializeField] public float TimeTillJumpApex { get; private set; } = .25f;
    [field: SerializeField] public float ApexThreshold { get; private set; } = .97f;
    [field: SerializeField] public float ApexHangTime { get; private set; } = .075f;
    [field: SerializeField] public float JumpBufferTime { get; private set; } = .125f;
    [field: SerializeField] public float JumpCoyoteTime { get; private set; } = .1f;
    
    [field: Header("Falling")]
    [field: SerializeField] public float TimeForUpwardsCancel { get; private set; } = 0.027f;
    [field: SerializeField] public float GravityReleaseMultiplier { get; private set; } = 2f;
    [field: SerializeField] public float MaxFallSpeed { get; private set; } = 25f;
    
    public float Gravity { get; private set; }
    public float InitialJumpVelocity  { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2 * AdjustedJumpHeight)/Mathf.Pow(TimeTillJumpApex, 2);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}