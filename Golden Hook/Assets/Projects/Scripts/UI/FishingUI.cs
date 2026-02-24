using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI passiveIncomeText;

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

    [Header("Zone Panel")]
    [SerializeField] private Transform zonePanelContent;
    [SerializeField] private GameObject zoneBottonPrefabs;

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

        catchPopupPanel?.SetActive(false);
        reelPrompt?.SetActive(false);
       
        StartCoroutine(InitAfterManager());
    }

    private System.Collections.IEnumerator InitAfterManager()
    {
        yield return null;

        _upgrade = UpgradeManager.Instance;

        RefreshAutoButton();
        RefreshUpgradeUI();
        RefreshZonePanel();
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
        castButton.interactable = false;
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
    }

    private void OnUpgradeBoat()
    {
        UpgradeManager.Instance?.TryUpgradeBoat();
        RefreshUpgradeUI();
    }

    private void OnHireWorker()
    {
        UpgradeManager.Instance.TryHireWorker();
        RefreshUpgradeUI();
    }

    private void OnMoneyChanged(MoneyChangedEvent e)
    {
        if (moneyText != null) moneyText.text = $"${e.NewAmount:N0}";
        RefreshUpgradeUI();
        RefreshZonePanel();
    }

    private void OnUpgradeEvent(UpgradeEvent e)
    {
        RefreshUpgradeUI();
        RefreshAutoButton();
    }

    private void OnZoneUnlocked(ZoneUnlockedEvent e) => RefreshZonePanel();

    private void RefreshUpgradeUI()
    {
        if (_upgrade == null) return;

        int rodCost = _upgrade.GetRodUpgradeCost();
        int boatCost = _upgrade.GetBoatUpgradeCost();
        int workerCost = _upgrade.GetWorkerHireCost();
        int money = EconomyManager.Instance?.CurrentMoney ?? 0;

        if (rodCostText != null) rodCostText.text = rodCost > 0 ? $"${rodCost}" : "MAX";
        if (boatCostText != null) boatCostText.text = boatCost > 0 ? $"${boatCost}" : "MAX";
        if (workerCostText != null) workerCostText.text = workerCost > 0 ? $"${workerCost}" : "N/A";
        if (workerCountText != null) workerCountText.text = $"Workers: {_upgrade.WorkerCount}/{_upgrade.MaxWorkers}";

        if (upgradeRodButton != null) upgradeRodButton.interactable = _upgrade.CanUpgradeRod() && money >= rodCost;
        if (upgradeBoatButton != null) upgradeBoatButton.interactable = _upgrade.CanUpgradeBoat() && money >= boatCost;
        if (hireWorkerButton != null) hireWorkerButton.interactable = _upgrade.CanHireWorker() && money >= workerCost;
    }

    private void RefreshZonePanel()
    {
        if (zonePanelContent == null || zoneBottonPrefabs == null) return;

        foreach (Transform child in zonePanelContent)
            Destroy(child.gameObject);

        foreach (var zone in ZoneManager.Instance.GetAllZones())
        {
            var go = Instantiate(zoneBottonPrefabs, zonePanelContent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();

            string label = zone.isUnlocked
                ? zone.zoneName
                : $"{zone.zoneName} (${zone.unlockCost})";

            if (txt != null) txt.text = label ;

            var capturedZone = zone;
            btn?.onClick.AddListener(() =>
            {
                if (capturedZone.isUnlocked)
                    ZoneManager.Instance.SwitchToZone(capturedZone);
                else
                    ZoneManager.Instance.TryUnlockZone(capturedZone);
            });

            btn.interactable = zone.isUnlocked ||
                (EconomyManager.Instance?.CurrentMoney ?? 0) >= zone.unlockCost;
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
                break;

            case FishingStateId.Casting:
            case FishingStateId.Waiting:
                castButton.interactable = false;
                reelButton.interactable = false;
                autoToggleButton.interactable = false;
                break;

            case FishingStateId.Hooked:
                reelButton?.gameObject.SetActive(true);
                reelButton.interactable = !_isAutoMode;
                autoToggleButton.interactable = false;
                break;

            case FishingStateId.ReelIn:
                reelButton.interactable = false;
                autoToggleButton.interactable = true;
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
}
