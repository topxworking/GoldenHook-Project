using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FishingUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI passiveIncomeText;
    [SerializeField] private TextMeshProUGUI equipmentText;

    [Header("Buttons")]
    [SerializeField] private Button castButton;
    [SerializeField] private Button reelButton;
    [SerializeField] private Button autoToggleButton;
    [SerializeField] private Button upgradeRodButton;
    [SerializeField] private Button upgradeBoatButton;
    [SerializeField] private Button hireWorkerButton;
    [SerializeField] private Image autoToggleImage;
    [SerializeField] private Sprite autoOnSprite;
    [SerializeField] private Sprite autoOffSprite;
    [SerializeField] private Sprite autoLockedSprite;

    [Header("Catch Popup")]
    [SerializeField] private GameObject catchPopupPanel;
    [SerializeField] private Image catchFishImage;
    [SerializeField] private TextMeshProUGUI catchFishName;
    [SerializeField] private TextMeshProUGUI catchFishRarity;
    [SerializeField] private TextMeshProUGUI catchFishPrice;

    [Header("Reel Prompt")]
    [SerializeField] private GameObject reelPrompt;

    [Header("Upgrade Costs")]
    [SerializeField] private TextMeshProUGUI rodCostText;
    [SerializeField] private TextMeshProUGUI boatCostText;
    [SerializeField] private TextMeshProUGUI workerCostText;
    [SerializeField] private TextMeshProUGUI workerCountText;

    [Header("Zone Buttons")]
    [SerializeField] private List<ZoneButtonEntry> zoneButtons = new();

    [System.Serializable]
    public class ZoneButtonEntry
    {
        public Button button;
        public Image buttonImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI activeLabel;
        public SeaZoneData zoneData;
    }

    private bool _isAutoMode = false;
    private float _popupTimer = 0f;

    private FishingController _fishing;
    private UpgradeManager _upgrade;

    private void Start()
    {
        _fishing = FindFirstObjectByType<FishingController>();
        _upgrade = UpgradeManager.Instance;

        castButton?.onClick.AddListener(OnCastPressed);
        reelButton?.onClick.AddListener(OnReelPressed);
        autoToggleButton?.onClick.AddListener(OnAutoToggle);
        upgradeRodButton?.onClick.AddListener(OnUpgradeRod);
        upgradeBoatButton?.onClick.AddListener(OnUpgradeBoat);
        hireWorkerButton?.onClick.AddListener(OnHireWorker);

        foreach (var entry in zoneButtons)
        {
            var captured = entry;
            captured.button?.onClick.AddListener(() => OnZoneButtonPressed(captured));
        }

        catchPopupPanel?.SetActive(false);
        reelPrompt?.SetActive(false);

        StartCoroutine(InitAfterManager());
    }

    private System.Collections.IEnumerator InitAfterManager()
    {
        yield return null;
        yield return null;

        _upgrade = UpgradeManager.Instance;

        RefreshAutoButton();
        RefreshUpgradeUI();
        RefreshZoneButtons();
        RefreshEquipmentText();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<MoneyChangedEvent>(OnMoneyChanged);
        EventManager.Subscribe<UpgradeEvent>(OnUpgradeEvent);
        EventManager.Subscribe<ZoneUnlockedEvent>(OnZoneUnlocked);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<MoneyChangedEvent>(OnMoneyChanged);
        EventManager.Unsubscribe<UpgradeEvent>(OnUpgradeEvent);
        EventManager.Unsubscribe<ZoneUnlockedEvent>(OnZoneUnlocked);
    }

    private void Update()
    {
        if (_popupTimer > 0f)
        {
            _popupTimer -= Time.deltaTime;
            if (_popupTimer <= 0f)
            {
                catchPopupPanel.SetActive(false);
            }
        }

        if (passiveIncomeText != null && EconomyManager.Instance != null)
            passiveIncomeText.text = $"+${EconomyManager.Instance.PassiveIncome:F2}/s passive income";
    }

    public void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    public void ShowReelPrompt(bool show) => reelPrompt?.SetActive(show);

    public void ShowCatchResult(CaughtFish fish)
    {
        if (catchPopupPanel == null) return;

        catchPopupPanel.SetActive(true);
        if (catchFishImage != null) catchFishImage.sprite = fish.Data.fishSprite;
        if (catchFishImage != null) catchFishImage.color = fish.Data.GetRarityColor();
        if (catchFishImage != null) catchFishName.text = fish.Data.fishName;
        if (catchFishRarity != null)
        {
            catchFishRarity.text = fish.Data.rarity.ToString();
            catchFishRarity.color = fish.Data.GetRarityColor();
        }
        if (catchFishPrice != null) catchFishPrice.text = $"+${fish.SellPrice} ({fish.Weight:F1}kg)";

        _popupTimer = 2.5f;
    }

    private void OnCastPressed()
    {
        _fishing?.OnPlayerTap();
    }

    private void OnReelPressed()
    {
        _fishing?.OnPlayerTap();
    }

    private void OnAutoToggle()
    {
        if (!IsAutoUnlocked())
        {
            SetStatus("Upgrade Rod to Level 2 to unlock Auto Fishing!");
            return;
        }

        _isAutoMode = !_isAutoMode;
        _fishing?.ToggleAutoFishing(_isAutoMode);
        castButton.interactable = !_isAutoMode;
        RefreshAutoButton();
        RefreshUpgradeUI();
    }

    private void OnUpgradeRod()
    {
        UpgradeManager.Instance?.TryUpgradeRod();
        RefreshUpgradeUI();
        RefreshEquipmentText();
    }

    private void OnUpgradeBoat()
    {
        UpgradeManager.Instance?.TryUpgradeBoat();
        RefreshUpgradeUI();
        RefreshEquipmentText();
    }

    private void OnHireWorker()
    {
        UpgradeManager.Instance.TryHireWorker();
        RefreshUpgradeUI();
        RefreshEquipmentText();
    }

    private void OnMoneyChanged(MoneyChangedEvent e)
    {
        if (moneyText != null) moneyText.text = $"${e.NewAmount:N0}";
        RefreshUpgradeUI();
        RefreshEquipmentText();
    }

    private void OnUpgradeEvent(UpgradeEvent e)
    {
        RefreshUpgradeUI();
        RefreshAutoButton();
        RefreshEquipmentText();
    }

    private void RefreshUpgradeUI()
    {
        if (_upgrade == null) return;

        int rodCost = _upgrade.GetRodUpgradeCost();
        int boatCost = _upgrade.GetBoatUpgradeCost();
        int workerCost = _upgrade.GetWorkerHireCost();
        int money = EconomyManager.Instance?.CurrentMoney ?? 0;

        if (rodCostText != null)        rodCostText.text = rodCost > 0 ? $"${rodCost}" : "MAX";
        if (boatCostText != null)       boatCostText.text = boatCost > 0 ? $"${boatCost}" : "MAX";
        if (workerCostText != null)     workerCostText.text = workerCost > 0 ? $"${workerCost}" : "N/A";
        if (workerCountText != null)    workerCountText.text = $"Workers: {_upgrade.WorkerCount}/{_upgrade.MaxWorkers}";

        if (upgradeRodButton != null)   upgradeRodButton.interactable = _upgrade.CanUpgradeRod() && money >= rodCost;
        if (upgradeBoatButton != null)  upgradeBoatButton.interactable = _upgrade.CanUpgradeBoat() && money >= boatCost;
        if (hireWorkerButton != null)   hireWorkerButton.interactable = _upgrade.CanHireWorker() && money >= workerCost;

        bool isIdle = _fishing == null ||
            _fishing.CurrentStateId == FishingStateId.Idle;

        if (upgradeRodButton != null)
            upgradeRodButton.interactable = isIdle && _upgrade.CanUpgradeRod() && money >= rodCost;
        if (upgradeBoatButton != null)
            upgradeBoatButton.interactable = isIdle && _upgrade.CanUpgradeBoat() && money >= boatCost;
        if (hireWorkerButton != null)
            hireWorkerButton.interactable = isIdle && _upgrade.CanHireWorker() && money >= workerCost;
    }

    private void OnZoneUnlocked(ZoneUnlockedEvent e) => RefreshZoneButtons();

    private void OnZoneButtonPressed(ZoneButtonEntry entry)
    {
        if (entry.zoneData == null) return;
        var zone = entry.zoneData;

        Debug.Log($"[ZoneBtn] {zone.zoneName} | zoneIndex={zone.zoneIndex} | unlockCost={zone.unlockCost} | money={EconomyManager.Instance?.CurrentMoney} | isUnlocked={ZoneManager.Instance.IsZoneUnlocked(zone)}");

        bool isUnlocked = ZoneManager.Instance.IsZoneUnlocked(zone);
        bool canAfford = (EconomyManager.Instance?.CurrentMoney ?? 0) >= zone.unlockCost;

        if (!isUnlocked && !canAfford)
        {
            SetStatus($"Need ${zone.unlockCost - (EconomyManager.Instance?.CurrentMoney ?? 0):N0} more to unlock {zone.zoneName}");
            return;
        }

        if (isUnlocked)
        {
            ZoneManager.Instance.SwitchToZone(zone);
            RefreshZoneButtons();
            return;
        }

        if (zone.requiredZoneIndex >= 0 && !ZoneManager.Instance.IsZoneUnlocked(zone.requiredZoneIndex))
        {
            var prereq = ZoneManager.Instance.GetAllZones()
                .Find(z => z.zoneIndex == zone.requiredZoneIndex);
            SetStatus($"Unlock {prereq?.zoneName ?? "previous zone"} first!");
            return;
        }

        int money = EconomyManager.Instance?.CurrentMoney ?? 0;
        if (money < zone.unlockCost)
        {
            SetStatus($"Need ${zone.unlockCost - money:N0} more to unlock {zone.zoneName}");
            return;
        }

        bool success = ZoneManager.Instance.TryUnlockZone(zone);
        if (success)
        {
            SetStatus($"{zone.zoneName} Unlocked!");
            RefreshZoneButtons();
        }
        else
        {
            SetStatus($"Need ${zone.unlockCost - (EconomyManager.Instance?.CurrentMoney ?? 0):N0} more to unlock {zone.zoneName}");
        }
    }

    public void RefreshZoneButtons()
    {
        if (ZoneManager.Instance == null) return;

        foreach (var entry in zoneButtons)
        {
            if (entry.zoneData == null || entry.button == null) continue;

            var zone = entry.zoneData;
            bool isUnlocked = ZoneManager.Instance.IsZoneUnlocked(zone);
            bool isCurrentZone = ZoneManager.Instance.CurrentZone == zone;
            bool canAfford = (EconomyManager.Instance?.CurrentMoney ?? 0) >= zone.unlockCost;

            if (entry.nameText != null)
            {
                entry.nameText.text = isUnlocked ? zone.zoneName : $"{zone.zoneName}";
                entry.nameText.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            if (entry.costText != null)
            {
                entry.costText.text = isCurrentZone ? "" : isUnlocked
                    ? $"x{zone.incomeMultiplier} income"
                    : $"${zone.unlockCost:N0}";

                entry.costText.color = canAfford && !isUnlocked
                    ? new Color(1f, 0.85f, 0.3f)
                    : new Color(0.7f, 0.7f, 0.7f);
            }

            if (entry.activeLabel != null)
            {
                entry.activeLabel.gameObject.SetActive(isCurrentZone);
                entry.activeLabel.text = "Active";
            }

            entry.button.interactable = isUnlocked || canAfford;
        }
    }

    public void UpdateFishingButtons(FishingStateId state)
    {
        switch (state)
        {
            case FishingStateId.Idle:
                castButton?.gameObject.SetActive(true);
                castButton.interactable = !_isAutoMode;
                reelButton.interactable = false;
                upgradeRodButton.interactable = true;
                upgradeBoatButton.interactable = true;
                hireWorkerButton.interactable = true;
                break;

            case FishingStateId.Casting:
                castButton.interactable = false;
                reelButton.interactable = false;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireWorkerButton.interactable = false;
                break;

            case FishingStateId.Waiting:
                castButton.interactable = false;
                reelButton.interactable = false;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireWorkerButton.interactable = false;
                break;

            case FishingStateId.Hooked:
                reelButton?.gameObject.SetActive(true);
                reelButton.interactable = !_isAutoMode;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireWorkerButton.interactable = false;
                break;

            case FishingStateId.ReelIn:
                reelButton.interactable = false;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireWorkerButton.interactable = false;
                break;
        }
    }

    public bool IsAutoUnlocked()
    {
        return UpgradeManager.Instance?.CurrentRod?.level >= 2;
    }

    private void RefreshAutoButton()
    {
        if (autoToggleButton == null) return;

        if (!IsAutoUnlocked())
        {
            autoToggleImage.sprite = autoLockedSprite;
            autoToggleButton.interactable = true;
        }
        else if (_isAutoMode)
        {
            autoToggleImage.sprite = autoOnSprite;
            autoToggleButton.interactable = true;
        }
        else
        {
            autoToggleImage.sprite = autoOffSprite;
            autoToggleButton.interactable = true;
        }
    }

    private void RefreshEquipmentText()
    {
        if (equipmentText == null || _upgrade == null) return;

        string rodName = _upgrade.CurrentRod?.rodName ?? "None";
        int rodLv = _upgrade.CurrentRod?.level ?? 0;
        string boatName = _upgrade.CurrentBoat?.boatName ?? "None";

        equipmentText.text = $"Rod: {rodName} Lv.{rodLv} | Boat: {boatName}";
    }
}
