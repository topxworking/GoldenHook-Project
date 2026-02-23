using System.Collections.Generic;
using UnityEngine;

public class FishingStateMachine
{
    private readonly Dictionary<FishingStateId, IFishingState> _state = new()
    {
        { FishingStateId.Idle, new IdleState() },
        { FishingStateId.Casting, new CastingState() },
        { FishingStateId.Waiting, new WaitingState() },
        { FishingStateId.Hooked, new HookedState() },
        { FishingStateId.ReelIn, new ReelInState() },
    };

    private IFishingState _current;
    private readonly FishingController _owner;

    public RodData CurrentRod => _owner.CurrentRod;
    public IFishingStrategy ActiveStrategy => _owner.ActiveStrategy;
    public FishingUI UI => _owner.FisingUI;
    public FishingStateId CurrentStateId => _current?.StateId ?? FishingStateId.Idle;

    public FishingStateMachine(FishingController owner)
    {
        _owner = owner;
        _current = _state[FishingStateId.Idle];
        _current.Enter(this);
    }

    public void TransitionTo(FishingStateId id)
    {
        if (!_state.TryGetValue(id, out var next)) return;

        _current?.Exit(this);
        _current = next;
        _current.Enter(this);
    }

    public void Update()
    {
        _current?.Update(this);
    }

    public void SendInput() => _current?.OnPlayerInput(this);
    public void CatchFish() => _owner.ProcessCatch();
    public void PlayAnimation(string anim) => _owner.PlayAnim(anim);
}

public class FishingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FishingUI fishingUI;
    [SerializeField] private Animator fishermanAnimator;

    [Header("Config")]
    [SerializeField] private Vector3 fishSpawnOffset = new(0, 1, 0);

    public RodData CurrentRod {  get; private set; }
    public IFishingStrategy ActiveStrategy { get; private set; }
    public FishingUI FisingUI => fishingUI;

    private FishingStateMachine _stateMachine;
    private SeaZoneData _currentZone;

    private void Start()
    {
        _stateMachine = new FishingStateMachine(this);
        SetStrategy(new ManualFishingStrategy(CurrentRod));
    }

    private void Update()
    {
        _stateMachine.Update();
        ActiveStrategy?.OnUpdate(this);
    }

    public void SetRod(RodData rod)
    {
        CurrentRod = rod;

        bool isAuto = ActiveStrategy is AutoFishingStrategy;
        SetStrategy(isAuto ? new AutoFishingStrategy(rod) : new ManualFishingStrategy(rod));
    }

    public void SetZone(SeaZoneData zone) => _currentZone = zone;

    public void SetStrategy(IFishingStrategy strategy)
    {
        ActiveStrategy?.OnDeactivate(this);
        ActiveStrategy = strategy;
        ActiveStrategy.OnActivate(this);
        fishingUI?.UpgradeStrategyLabel(strategy.StrategyName);
    }

    public void ToggleAutoFishing(bool enable)
    {
        SetStrategy(enable
            ? (IFishingStrategy)new AutoFishingStrategy(CurrentRod)
            : new ManualFishingStrategy(CurrentRod));
    }

    public void OnPlayerTap() => _stateMachine.SendInput();

    public void TriggerAutoCast()
    {
        if (_stateMachine.CurrentStateId == FishingStateId.Idle)
            _stateMachine.TransitionTo(FishingStateId.Casting);
    }

    public void ProcessCatch()
    {
        if (_currentZone == null) { Debug.LogWarning("No zone set!"); return; }

        var fish = FishFactory.CreateFish(_currentZone, CurrentRod);
        if (fish == null) return;

        FishPool.Instance?.Get(fish.Data, transform.position + fishSpawnOffset);

        EventManager.Publish(new FishCaughtEvent
        {
            FishData = fish.Data,
            Weight = fish.Weight,
            SellPrice = fish.SellPrice,
        });

        fishingUI?.ShowCatchResult(fish);
    }

    public void PlayAnim(string anim)
    {
        if (fishermanAnimator != null)
            fishermanAnimator.SetTrigger(anim);
    }
}
