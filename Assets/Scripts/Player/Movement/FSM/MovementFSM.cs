using System.Collections.Generic;
using UnityEngine;

public class MovementFSM
{
    private Dictionary<MovementStateType, MovementState> _movementStates;
    private MovementState _currentState;

    public MovementFSM(PlayerMovementContext movementContext, PlayerMovementController movementController)
    {
        _movementStates = new Dictionary<MovementStateType, MovementState>
        {
            { MovementStateType.Idle, new IdleState(movementContext, movementController.InvokeOnEnterIdle) },
            { MovementStateType.Walking, new WalkingState(movementContext, movementController.InvokeOnStartWalking) },
            { MovementStateType.Jumping, new JumpingState(movementContext, movementController.InvokeOnInitiateJump, movementController.InvokeOnBumpHead, movementController.InvokeOnJumpEnded) },
            { MovementStateType.Falling, new FallingState(movementContext, movementController.InvokeOnStartFalling, movementController.InvokeOnLand) },
            { MovementStateType.Interacting, new InteractionState(movementContext, movementContext.Interaction.OnBeginInteraction, movementContext.Interaction.OnEndInteraction, movementContext.Interaction.OnInteract) }
        };
        Initialize();
    }

    public void Update()
    {
        _currentState.Update();
    }

    public void FixedUpdate()
    {
        _currentState.FixedUpdate();
        
        MovementStateType? nextStateType = _currentState.NextState();
        Debug.Log($"Current State: {_currentState.GetType().Name}, Next State: {nextStateType}");
        if(nextStateType != null)
        {
            ChangeState((MovementStateType)nextStateType);
        }
    }
    
    public void ChangeState(MovementStateType newState)
    {
        _currentState.Exit();
        _currentState = _movementStates[newState];
        _currentState.Enter();
    }

    private void Initialize()
    {
        _currentState = _movementStates[MovementStateType.Idle];
        _currentState.Enter();
    }
}