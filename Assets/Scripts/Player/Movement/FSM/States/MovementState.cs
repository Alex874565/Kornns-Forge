public class MovementState
{
    protected PlayerMovementContext Ctx;

    protected MovementState(PlayerMovementContext ctx)
    {
        Ctx = ctx;
    }
    
    public virtual void Enter(){}
    public virtual void Update(){}
    public virtual void FixedUpdate(){}
    public virtual void Exit(){}

    public virtual MovementStateType? NextState()
    {
        return null;
    }
}