using System.Collections.Generic;

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
            { MovementStateType.Jumping, new JumpingState(movementContext, movementController.InvokeOnInitiateJump, movementController.InvokeOnBumpHead) },
            { MovementStateType.Falling, new FallingState(movementContext, movementController.InvokeOnStartFalling, movementController.InvokeOnLand) },
        };
        Initialize();
    }

    public void Update()
    {
        _currentState.Update();
        MovementStateType? nextStateType = _currentState.NextState();
        if(nextStateType != null)
        {
            ChangeState((MovementStateType)nextStateType);
        }
    }

    public void FixedUpdate()
    {
        _currentState.FixedUpdate();
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