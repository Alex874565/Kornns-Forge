using System;
using UnityEngine;

public class InteractionState : MovementState
{
    private readonly Action _onStartInteraction;
    private readonly Action _onEndInteraction;
    private readonly Action _onInteract;

    private float _timeSinceInteraction;
    private float _interactionTime = .5f;
    
    public InteractionState(PlayerMovementContext ctx, Action onStartInteraction, Action onEndInteraction, Action onInteract) : base(ctx)
    {
        _onStartInteraction = onStartInteraction;
        _onEndInteraction = onEndInteraction;
        _onInteract = onInteract;
    }

    public override void Enter()
    {
        _timeSinceInteraction = 0;
        _onStartInteraction?.Invoke();
    }

    public override void Update()
    {
        _timeSinceInteraction += Time.deltaTime;
        if (_timeSinceInteraction >= _interactionTime)
        {
            _timeSinceInteraction = 0;
            _onInteract?.Invoke();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        _onEndInteraction?.Invoke();
    }

    public override MovementStateType? NextState()
    {
        if(!Ctx.Interaction.IsInteracting)
        {
            return MovementStateType.Idle;
        }

        return null;
    }
}