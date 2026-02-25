using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI passiveIncomeText;
    [SerializeField] private TextMeshProUGUI equipmentText;
    [SerializeField] private GameObject autoModePopup;

    [Header("Buttons")]
    [SerializeField] private Button castButton;
    [SerializeField] private Button reelButton;
    [SerializeField] private Button autoToggleButton;
    [SerializeField] private Button upgradeRodButton;
    [SerializeField] private Button upgradeBoatButton;
    [SerializeField] private Button hireCrewButton;

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
    [SerializeField] private TextMeshProUGUI crewCostText;
    [SerializeField] private TextMeshProUGUI crewCountText;

    [Header("Zone Panel")]
    [SerializeField] private RectTransform zonePanelRect;
    [SerializeField] private Button zoneMunuButton;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private List<ZoneButtonEntry> zoneButtons = new();

    [Header("Debug")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Button debugMenuButton;
    [SerializeField] private Button debugAddMoneyButton;
    [SerializeField] private GameObject debugInputPanel;
    [SerializeField] private TMP_InputField debugInputField;
    [SerializeField] private Button debugConfirmButton;
    [SerializeField] private Button debugCancelButton;

    [System.Serializable]
    public class ZoneButtonEntry
    {
        public Button button;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI activeLabel;
        public SeaZoneData zoneData;
    }

    private bool _isAutoMode = false;
    private float _popupTimer = 0f;
    private bool _isFishing = false;
    private bool _isZonePanelOpen = false;
    private Coroutine _slideCoroutine;

    private float _panelHiddenX;
    private float _panelShownX;

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
        hireCrewButton?.onClick.AddListener(OnHireWorker);
        zoneMunuButton?.onClick.AddListener(OnZoneMenuToggle);

        debugMenuButton?.onClick.AddListener(OnDebugMenuToggle);
        debugAddMoneyButton?.onClick.AddListener(OnDebugAddMoneyPressed);
        debugConfirmButton?.onClick.AddListener(OnDebugConfirm);
        debugCancelButton?.onClick.AddListener(OnDebugCancel);
        debugInputField?.onSubmit.AddListener(_ => OnDebugConfirm());

        debugPanel?.SetActive(false);
        debugInputPanel?.SetActive(false);

        foreach (var entry in zoneButtons)
        {
            var captured = entry;
            captured.button?.onClick.AddListener(() => OnZoneButtonPressed(captured));
        }

        if (zonePanelRect != null)
        {
            float panelWidth = zonePanelRect.rect.width;
            _panelShownX = zonePanelRect.anchoredPosition.x;
            _panelHiddenX = _panelHiddenX - panelWidth;

            zonePanelRect.anchoredPosition = new Vector2(_panelHiddenX, zonePanelRect.anchoredPosition.y);
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
        _fishing = FindFirstObjectByType<FishingController>();

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
            passiveIncomeText.text = $"+${EconomyManager.Instance.PassiveIncome:F2}/s passive";
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
        if (catchFishName != null) catchFishName.text = fish.Data.fishName;

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

        if (!_isAutoMode) _isFishing = false;

        _fishing?.ToggleAutoFishing(_isAutoMode);
        castButton.interactable = !_isAutoMode;
        RefreshAutoButton();
        RefreshUpgradeUI();

        ShowAutoModePopup(_isAutoMode);
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

        if (_upgrade == null) _upgrade = UpgradeManager.Instance;

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
        if (crewCostText != null)     crewCostText.text = workerCost > 0 ? $"${workerCost}" : "N/A";
        if (crewCountText != null)    crewCountText.text = $"Crew: {_upgrade.WorkerCount}/{_upgrade.MaxWorkers}";

        bool isIdle = _fishing == null ||
            _fishing.CurrentStateId == FishingStateId.Idle;

        if (upgradeRodButton != null)
            upgradeRodButton.interactable = isIdle && _upgrade.CanUpgradeRod() && money >= rodCost;
        if (upgradeBoatButton != null)
            upgradeBoatButton.interactable = isIdle && _upgrade.CanUpgradeBoat() && money >= boatCost;
        if (hireCrewButton != null)
            hireCrewButton.interactable = isIdle && _upgrade.CanHireWorker() && money >= workerCost;
    }

    private void OnZoneUnlocked(ZoneUnlockedEvent e) => RefreshZoneButtons();

    private void OnZoneButtonPressed(ZoneButtonEntry entry)
    {
        if (entry.zoneData == null) return;

        if (_isAutoMode)
        {
            SetStatus("Turn off Auto Fishing before switching zones!");
            return;
        }

        if (_isFishing)
        {
            SetStatus("Wait until fishing is done before switching zones!");
            return;
        }

        var zone = entry.zoneData;

        bool isUnlocked = ZoneManager.Instance.IsZoneUnlocked(zone);

        if (isUnlocked)
        {
            ZoneManager.Instance.SwitchToZone(zone);
            RefreshZoneButtons();

            if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
            _isZonePanelOpen = false;
            _slideCoroutine = StartCoroutine(SlidePanel(_panelHiddenX));
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
            zone.isUnlocked = true;
            RefreshZoneButtons();
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
                    ? $"x{zone.incomeMultiplier}"
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

            entry.button.interactable = true;
        }
    }

    public void UpdateFishingButtons(FishingStateId state)
    {
        switch (state)
        {
            case FishingStateId.Idle:
                _isFishing = false;
                castButton?.gameObject.SetActive(true);
                castButton.interactable = !_isAutoMode;
                reelButton.interactable = false;
                autoToggleButton.interactable = true;
                RefreshUpgradeUI();
                break;

            case FishingStateId.Casting:
                _isFishing = true;
                castButton.interactable = false;
                reelButton.interactable = false;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireCrewButton.interactable = false;
                break;

            case FishingStateId.Waiting:
                _isFishing = true;
                castButton.interactable = false;
                reelButton.interactable = false;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireCrewButton.interactable = false;
                break;

            case FishingStateId.Hooked:
                _isFishing = true;
                reelButton?.gameObject.SetActive(true);
                reelButton.interactable = !_isAutoMode;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireCrewButton.interactable = false;
                break;

            case FishingStateId.ReelIn:
                _isFishing = true;
                reelButton.interactable = false;
                upgradeRodButton.interactable = false;
                upgradeBoatButton.interactable = false;
                hireCrewButton.interactable = false;
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
            autoToggleButton.interactable = false;
        else
            autoToggleButton.interactable = true;
    }

    private void RefreshEquipmentText()
    {
        if (equipmentText == null || _upgrade == null) return;

        string rodName = _upgrade.CurrentRod?.rodName ?? "None";
        int rodLv = _upgrade.CurrentRod?.level ?? 0;
        string boatName = _upgrade.CurrentBoat?.boatName ?? "None";

        equipmentText.text = $"Rod: {rodName} Lv.{rodLv} | Boat: {boatName}";
    }

    private void ShowAutoModePopup(bool isAuto)
    {
        if (autoModePopup == null) return;

        autoModePopup.SetActive(isAuto);
    }

    private void OnDebugMenuToggle()
    {
        if (debugPanel == null) return;
        debugPanel.SetActive(!debugPanel.activeSelf);
        debugInputPanel?.SetActive(false);
    }

    private void OnDebugAddMoneyPressed()
    {
        if (debugInputPanel == null) return;
        debugInputPanel.SetActive(true);
        debugInputField?.SetTextWithoutNotify("");
        debugInputField?.ActivateInputField();
    }

    private void OnDebugConfirm()
    {
        if (debugInputField == null) return;

        string raw = debugInputField.text.Trim();
        if (int.TryParse(raw, out int amount) && amount > 0)
        {
            EconomyManager.Instance?.AddMoney(amount);
            SetStatus($"[DEBUG] +${amount:N0}");
        }
        else
        {
            SetStatus("[DEBUG] Invalid amount");
        }

        debugInputPanel?.SetActive(false);
        debugInputField?.SetTextWithoutNotify("");
    }

    private void OnDebugCancel()
    {
        debugInputPanel?.SetActive(false);
        debugInputField?.SetTextWithoutNotify("");
    }

    private void OnZoneMenuToggle()
    {
        _isZonePanelOpen = !_isZonePanelOpen;

        if (_slideCoroutine != null)
            StopCoroutine(_slideCoroutine);

        _slideCoroutine = StartCoroutine(
            SlidePanel(_isZonePanelOpen ? _panelShownX : _panelHiddenX)
        );
    }

    private IEnumerator SlidePanel(float targetX)
    {
        float startX = zonePanelRect.anchoredPosition.x;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            float x = Mathf.Lerp(startX, targetX, t);
            zonePanelRect.anchoredPosition = new Vector2(x, zonePanelRect.anchoredPosition.y);
            yield return null;
        }

        zonePanelRect.anchoredPosition = new Vector2(targetX, zonePanelRect.anchoredPosition.y);
        _slideCoroutine = null;
    }
}
