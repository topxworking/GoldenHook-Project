using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private ZoneManager    zoneManager;
    [SerializeField] private FishPool       fishPool;

    [Header("Save Keys")]
    private const string KEY_MONEY      = "save_money";
    private const string KEY_ROD_LEVEL  = "save_rod_level";
    private const string KEY_BOAT_LEVEL = "save_boat_level";
    private const string KEY_WORKERS    = "save_worker_count";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        economyManager  ??= FindFirstObjectByType<EconomyManager>();
        upgradeManager  ??= FindFirstObjectByType<UpgradeManager>();
        zoneManager     ??= FindFirstObjectByType<ZoneManager>();
        fishPool        ??= FindFirstObjectByType<FishPool>();
    }

    private void Start()
    {
        LoadGame();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt(KEY_MONEY, EconomyManager.Instance?.CurrentMoney ?? 0);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(KEY_MONEY)) return;

        int savedMoney = PlayerPrefs.GetInt(KEY_MONEY, 0);
        EconomyManager.Instance?.AddMoney(savedMoney);
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
    }
}
