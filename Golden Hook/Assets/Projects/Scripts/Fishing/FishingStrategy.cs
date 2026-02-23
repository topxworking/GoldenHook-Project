using System;
using UnityEngine;

public interface IFishingStrategy
{
    string StrategyName { get; }
    float CatchInterval { get; }
    bool RequiresInput { get; }

    void OnActivate(FishingController controller);

    void OnUpdate(FishingController controller);

    void OnDeactivate(FishingController controller);
}

public class ManualFishingStrategy : IFishingStrategy
{
    public string StrategyName => "Manual";
    public bool RequiresInput => true;

    private readonly RodData _rod;
    public float CatchInterval => _rod != null ? _rod.autoFishInterval * 1.5f : 8f;

    public ManualFishingStrategy(RodData rod) => _rod = rod;

    public void OnActivate(FishingController controller) { }

    public void OnUpdate(FishingController controller) { }

    public void OnDeactivate(FishingController controller) { }   
}

public class AutoFishingStrategy : IFishingStrategy
{
    public string StrategyName => "Auto";
    public bool RequiresInput => false;

    private readonly RodData _rod;
    public float CatchInterval => _rod != null ? _rod.autoFishInterval : 12f;

    private float _autoTimer = 0f;

    public AutoFishingStrategy(RodData rod) => _rod = rod;

    public void OnActivate(FishingController controller) => _autoTimer = 0f;

    public void OnUpdate(FishingController controller)
    {
        _autoTimer += Time.deltaTime;
        if (_autoTimer >= CatchInterval)
        {
            _autoTimer = 0f;
            controller.TriggerAutoCast();
        }
    }

    public void OnDeactivate(FishingController controller) => _autoTimer = 0f;
}
