using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Starting Equipment")]
    [SerializeField] private RodData startingRod;
    [SerializeField] private BoatData startingBoat;
    [SerializeField] private WorkerData workerPrefabData;

    [Header("References")]
    [SerializeField] private FishingController fishingController;

    public RodData CurrentRod { get; private set; }
    public BoatData CurrentBoat { get; private set; }

    public RodData StartingRod { get; private set; }
    public BoatData StartingBoat { get; private set; }

    private readonly List<WorkerData> _hiredWorkers = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartingRod = startingRod;
        StartingBoat = startingBoat;
        CurrentRod = startingRod;
        CurrentBoat = startingBoat;
        fishingController?.SetRod(CurrentRod);
        RecalculatePassiveIncome();
    }

    // Rod Upgrade
    public bool TryUpgradeRod()
    {
        if (CurrentRod?.nextUpgrade == null)
        {
            Debug.Log("[Upgrade] Rod is already max level.");
            return false;
        }

        int cost = CurrentRod.nextUpgrade.upgradeCost;
        if (!EconomyManager.Instance.TrySpend(cost)) return false;

        CurrentRod = CurrentRod.nextUpgrade;
        CurrentRod.isUnlocked = true;
        fishingController?.SetRod(CurrentRod);

        EventManager.Publish(new UpgradeEvent { UpgradeType = "Rod", NewLevel = CurrentRod.level });
        Debug.Log($"[Upgrade] Rod >> {CurrentRod.rodName}");
        return true;
    }

    // Boat Upgrade
    public bool TryUpgradeBoat()
    {
        if (CurrentBoat?.nextUpgrade == null)
        {
            Debug.Log("[Upgrade] Boat is already max level");
            return false;
        }

        int cost = CurrentBoat.nextUpgrade.upgradeCost;
        if (!EconomyManager.Instance.TrySpend(cost)) return false;

        CurrentBoat = CurrentBoat.nextUpgrade;
        CurrentBoat.isUnlocked = true;

        EventManager.Publish(new UpgradeEvent { UpgradeType = "Boat", NewLevel = CurrentBoat.level });
        RecalculatePassiveIncome();
        return true;
    }

    // Worker Hire / Upgrade
    public bool TryHireWorker()
    {
        if (workerPrefabData == null) return false;
        if (_hiredWorkers.Count >= (CurrentBoat?.workerSlots ?? 1))
        {
            Debug.Log("[Upgrade] Worker slots full. Upgrade boat frist.");
            return false;
        }

        int cost = workerPrefabData.hireCost;
        if (!EconomyManager.Instance.TrySpend(cost)) return false;

        _hiredWorkers.Add(workerPrefabData);

        EventManager.Publish(new UpgradeEvent { UpgradeType = "Worker", NewLevel = _hiredWorkers.Count });
        RecalculatePassiveIncome();
        return true;
    }

    public void UpgradeRodFree()
    {
        if (CurrentRod?.nextUpgrade == null) return;
        CurrentRod = CurrentRod.nextUpgrade;
        CurrentRod.isUnlocked = true;
        fishingController?.SetRod(CurrentRod);
        EventManager.Publish(new UpgradeEvent { UpgradeType = "Rod", NewLevel = CurrentRod.level });
    }

    public void UpgradeBoatFree()
    {
        if (CurrentBoat?.nextUpgrade == null) return;
        CurrentBoat = CurrentBoat.nextUpgrade;
        CurrentBoat.isUnlocked = true;
        EventManager.Publish(new UpgradeEvent { UpgradeType = "Boat", NewLevel = CurrentBoat.level });
        RecalculatePassiveIncome();
    }

    public void HireWorkerFree()
    {
        if (workerPrefabData == null) return;
        if (_hiredWorkers.Count >= (CurrentBoat?.workerSlots ?? 1)) return;
        _hiredWorkers.Add(workerPrefabData);
        EventManager.Publish(new UpgradeEvent { UpgradeType = "Worker", NewLevel = _hiredWorkers.Count });
        RecalculatePassiveIncome();
    }

    // Passive Income Calcutation
    public void RecalculatePassiveIncome()
    {
        float income = 0f;

        foreach (var worker in _hiredWorkers)
            income += worker.incomeBonus * (CurrentBoat?.incomeMultiplier ?? 1f) * 10f;

        EconomyManager.Instance?.SetPassiveIncome(income);
        Debug.Log($"[Upgrade] Passive income/s = ${income:F2}");
    }

    public int GetRodUpgradeCost() => CurrentRod?.nextUpgrade?.upgradeCost ?? -1;
    public int GetBoatUpgradeCost() => CurrentBoat?.nextUpgrade?.upgradeCost ?? -1;
    public int GetWorkerHireCost() => workerPrefabData?.hireCost ?? -1;
    public bool CanUpgradeRod() => CurrentRod?.nextUpgrade != null;
    public bool CanUpgradeBoat() => CurrentBoat?.nextUpgrade != null;
    public bool CanHireWorker() => _hiredWorkers.Count < (CurrentBoat?.workerSlots ?? 1);
    public int WorkerCount => _hiredWorkers.Count;
    public int MaxWorkers => CurrentBoat?.workerSlots ?? 1;

}
