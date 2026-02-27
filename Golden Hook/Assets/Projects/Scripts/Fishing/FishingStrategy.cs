using System;
using UnityEngine;

public interface IFishingStrategy
{
    float CatchInterval { get; }
    bool RequiresInput { get; }

    void OnActivate(FishingController controller);

    void OnUpdate(FishingController controller);

    void OnDeactivate(FishingController controller);
}

public class ManualFishingStrategy : IFishingStrategy
{
    public bool RequiresInput => true;

    private readonly RodData _rod;
    public float CatchInterval => _rod != null ? _rod.autoFishInterval : 8f;

    public ManualFishingStrategy(RodData rod) => _rod = rod;

    public void OnActivate(FishingController controller) { }

    public void OnUpdate(FishingController controller) { }

    public void OnDeactivate(FishingController controller) { }   
}

public class AutoFishingStrategy : IFishingStrategy
{
    public bool RequiresInput => false;

    private readonly RodData _rod;
    public float CatchInterval => _rod != null ? _rod.autoFishInterval * 1.5f : 12f;

    private float _autoTimer = 0f;

    public AutoFishingStrategy(RodData rod) => _rod = rod;

    public void OnActivate(FishingController controller) => _autoTimer = 0f;

    public void OnUpdate(FishingController controller)
    {
        if (controller.CurrentStateId != FishingStateId.Idle) return;

        _autoTimer += Time.deltaTime;

        Debug.Log($"[Auto] timer={_autoTimer:F1}/{CatchInterval:F1} | rod={_rod?.rodName}");

        if (_autoTimer >= CatchInterval)
        {
            _autoTimer = 0f;
            controller.TriggerAutoCast();
        }
    }

    public void OnDeactivate(FishingController controller) => _autoTimer = 0f;
}
