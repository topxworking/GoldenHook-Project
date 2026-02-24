using UnityEngine;

public enum FishingStateId { Idle, Casting, Waiting, Hooked, ReelIn }

public interface IFishingState
{
    FishingStateId StateId { get; }
    void Enter(FishingStateMachine sm);
    void Update(FishingStateMachine sm);
    void Exit(FishingStateMachine sm);
    void OnPlayerInput(FishingStateMachine sm);
}

// Idle
public class IdleState : IFishingState
{
    public FishingStateId StateId => FishingStateId.Idle;

    public void Enter(FishingStateMachine sm)
    {
        sm.UI?.SetStatus("Ready to fish!");
        sm.UI?.UpdateFishingButtons(FishingStateId.Idle);
    }
    public void Exit(FishingStateMachine sm) { }
    public void Update(FishingStateMachine sm) { }

    public void OnPlayerInput(FishingStateMachine sm) =>
        sm.TransitionTo(FishingStateId.Casting);
}

// Casting
public class CastingState : IFishingState
{
    public FishingStateId StateId => FishingStateId.Casting;
    private float _castTime;

    public void Enter(FishingStateMachine sm)
    {
        _castTime = 0.5f / (sm.CurrentRod?.castingSpeed ?? 1f);
        sm.UI?.SetStatus("Casting...");
        sm.PlayAnimation("Cast");
        sm.UI?.UpdateFishingButtons(FishingStateId.Casting);
    }

    public void Update(FishingStateMachine sm)
    {
        _castTime -= Time.deltaTime;
        if (_castTime <= 0f)
            sm.TransitionTo(FishingStateId.Waiting);
    }

    public void Exit(FishingStateMachine sm) { }
    public void OnPlayerInput(FishingStateMachine sm) { }
}

// Waiting
public class WaitingState : IFishingState
{
    public FishingStateId StateId => FishingStateId.Waiting;
    private float _waitTimer;
    private float _biteWindow;

    public void Enter(FishingStateMachine sm)
    {
        float baseWait = sm.ActiveStrategy?.CatchInterval ?? 8f;
        _waitTimer = 0f;
        _biteWindow = baseWait * Random.Range(0.6f, 1.0f);
        sm.UI?.SetStatus("Waiting for a bite...");
        sm.PlayAnimation("Wait");
        sm.UI?.UpdateFishingButtons(FishingStateId.Waiting);
    }

    public void Update(FishingStateMachine sm)
    {
        _waitTimer += Time.deltaTime;
        if (_waitTimer >= _biteWindow)
            sm.TransitionTo(FishingStateId.Hooked);
    }

    public void Exit(FishingStateMachine sm) { }
    public void OnPlayerInput(FishingStateMachine sm) { }
}

// Hooked
public class HookedState : IFishingState
{
    public FishingStateId StateId => FishingStateId.Hooked;
    private float _missWindow = 2.5f;
    private float _timer;

    public void Enter(FishingStateMachine sm)
    {
        _timer = 0f;
        sm.UI?.SetStatus("Fish on the hook! REEL IT IN!");
        sm.PlayAnimation("Hooked");
        sm.UI?.ShowReelPrompt(true);
        sm.UI?.UpdateFishingButtons(FishingStateId.Hooked);
    }

    public void Update(FishingStateMachine sm)
    {
        _timer += Time.deltaTime;

        if (sm.ActiveStrategy != null && !sm.ActiveStrategy.RequiresInput)
        {
            sm.TransitionTo(FishingStateId.ReelIn);
            return;
        }

        if (_timer >= _missWindow)
        {
            sm.UI?.SetStatus("The fish got away!");
            sm.TransitionTo(FishingStateId.Idle);
        }
    }

    public void Exit(FishingStateMachine sm) => sm.UI?.ShowReelPrompt(false);

    public void OnPlayerInput(FishingStateMachine sm) =>
        sm.TransitionTo(FishingStateId.ReelIn);
}

// ReelIn
public class ReelInState : IFishingState
{
    public FishingStateId StateId => FishingStateId.ReelIn;
    private float _reelTime;

    public void Enter(FishingStateMachine sm)
    {
        _reelTime = 1.2f / (sm.CurrentRod?.castingSpeed ?? 1f);
        sm.UI?.SetStatus("Reeling in...");
        sm.PlayAnimation("ReelIn");
        sm.UI?.UpdateFishingButtons(FishingStateId.ReelIn);
    }

    public void Update(FishingStateMachine sm)
    {
        _reelTime -= Time.deltaTime;
        if (_reelTime <= 0f)
        {
            sm.CatchFish();
            sm.TransitionTo(FishingStateId.Idle);
        }
    }

    public void Exit(FishingStateMachine sm) { }
    public void OnPlayerInput(FishingStateMachine sm) { }
}
