using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int startingMoney = 0;
    public const int MaxMoney = 2147000000;

    public int CurrentMoney { get; private set; }

    private float _passiveIncomePerSecond = 0f;
    private float _passiveTimer = 0f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        SetMoney(startingMoney);
    }

    private void OnEnable()
    {
        EventManager.Subscribe<FishCaughtEvent>(OnFishCaught);
        EventManager.Subscribe<UpgradeEvent>(OnUpgrade);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<FishCaughtEvent>(OnFishCaught);
        EventManager.Unsubscribe<UpgradeEvent>(OnUpgrade);
    }

    private void Update()
    {
        if (_passiveIncomePerSecond <= 0f) return;

        _passiveTimer += Time.deltaTime;
        if (_passiveTimer > 1f)
        {
            _passiveTimer -= 1f;
            AddMoney(Mathf.RoundToInt(_passiveIncomePerSecond));
        }
    }

    private void OnFishCaught(FishCaughtEvent e)
    {
        AddMoney(e.SellPrice);
    }

    private void OnUpgrade(UpgradeEvent e)
    {
        RecalculatePassiveIncome();
    }

    public bool TrySpend(int amount)
    {
        if (CurrentMoney < amount) return false;
        AddMoney(-amount);
        return true;
    }

    public void AddMoney(int amount)
    {
        int old = CurrentMoney;

        long potentialTotal = (long)CurrentMoney + amount;

        CurrentMoney = (int)Math.Clamp(potentialTotal, 0, MaxMoney);

        EventManager.Publish(new MoneyChangedEvent
        {
            OldAmount = old,
            NewAmount = CurrentMoney,
            Delta = CurrentMoney - old,
        });
    }

    public void SetMoney(int amount)
    {
        int old = CurrentMoney;

        CurrentMoney = Mathf.Clamp(amount, 0, MaxMoney);

        EventManager.Publish(new MoneyChangedEvent
        {
            OldAmount = old,
            NewAmount = CurrentMoney,
            Delta = CurrentMoney - old,
        });
    }

    public void ResetMoney()
    {
        CurrentMoney = 0;
        SetPassiveIncome(0f);
        EventManager.Publish(new MoneyChangedEvent { NewAmount = 0 });
    }

    public float PassiveIncome { get; private set; }

    public void SetPassiveIncome(float amount)
    {
        PassiveIncome = amount;
        _passiveIncomePerSecond = amount;
    }

    private void RecalculatePassiveIncome()
    {
        UpgradeManager.Instance?.RecalculatePassiveIncome();
    }
}
