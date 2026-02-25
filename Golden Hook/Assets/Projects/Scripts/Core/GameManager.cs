using System.Collections.Generic;
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
    private const string KEY_ROD_INDEX  = "save_rod_index";
    private const string KEY_BOAT_INDEX = "save_boat_index";
    private const string KEY_WORKER_COUNT    = "save_worker_count";
    private const string KEY_ZONE_PREFIX = "save_zone";

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
        StartCoroutine(LoadGameDelayed());
    }

    private System.Collections.IEnumerator LoadGameDelayed()
    {
        yield return null;
        yield return null;
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
        PlayerPrefs.SetInt(KEY_ROD_INDEX, GetRodIndex());
        PlayerPrefs.SetInt(KEY_BOAT_INDEX, GetBoatIndex());
        PlayerPrefs.SetInt(KEY_WORKER_COUNT, upgradeManager?.WorkerCount ?? 0);

        var indexes = new List<int>();
        foreach (var zone in zoneManager.GetAllZones())
            if (ZoneManager.Instance.IsZoneUnlocked(zone))
                indexes.Add(zone.zoneIndex);

        PlayerPrefs.SetString("save_zone", string.Join(",", indexes));

        PlayerPrefs.Save();
        Debug.Log($"[Save] money={EconomyManager.Instance?.CurrentMoney} | zones={string.Join(",", indexes)}");
    }

    public void LoadGame()
    {
        Debug.Log($"[Load] HasKey={PlayerPrefs.HasKey(KEY_MONEY)} | EconomyReady={EconomyManager.Instance != null}");

        if (!PlayerPrefs.HasKey(KEY_MONEY))
        {
            Debug.Log("[Load] No save found — starting fresh");
            return;
        }

        int savedMoney = PlayerPrefs.GetInt(KEY_MONEY, 0);
        EconomyManager.Instance?.AddMoney(savedMoney);
        Debug.Log($"[Load] money={savedMoney}");

        int rodIndex = PlayerPrefs.GetInt(KEY_ROD_INDEX, 0);
        ApplyRodIndex(rodIndex);
        Debug.Log($"[Load] rodIndex={rodIndex}");

        int boatIndex = PlayerPrefs.GetInt(KEY_BOAT_INDEX, 0);
        ApplyBoatIndex(boatIndex);
        Debug.Log($"[Load] boatIndex={boatIndex}");

        string zonesStr = PlayerPrefs.GetString("save_zones", "");
        var indexes = new List<int>();
        foreach (var zone in zoneManager.GetAllZones())
            if (zone.isUnlocked)
                indexes.Add(zone.zoneIndex);
        Debug.Log($"[Load] zones={zonesStr}");

        if (!string.IsNullOrEmpty(zonesStr))
        {
            foreach (var s in zonesStr.Split(','))
                if (int.TryParse(s.Trim(), out int idx) && !indexes.Contains(idx))
                    indexes.Add(idx);
        }

        zoneManager?.LoadUnlockedZones(indexes);

        int workerCount = PlayerPrefs.GetInt(KEY_WORKER_COUNT, 0);
        for (int i = 0; i < workerCount; i++)
            upgradeManager?.HireWorkerFree();
        Debug.Log($"[Load] workers={workerCount}");
    }

    private int GetRodIndex()
    {
        if (upgradeManager == null) return 0;
        var startRod = upgradeManager.StartingRod;
        var current = upgradeManager.CurrentRod;
        return CountChainIndex(startRod, current,
            (r) => (r as RodData)?.nextUpgrade);
    }

    private int GetBoatIndex()
    {
        if (upgradeManager == null) return 0;
        var startBoat = upgradeManager.StartingBoat;
        var current = upgradeManager.CurrentBoat;
        return CountChainIndex(startBoat, current,
            (b) => (b as BoatData)?.nextUpgrade);
    }

    private int CountChainIndex<T>(T start, T current, System.Func<T, T> getNext) where T : class
    {
        int index = 0;
        var node = start;
        while (node != null && node != current)
        {
            node = getNext(node);
            index++;
        }
        return index;
    }

    private void ApplyRodIndex(int index)
    {
        if (upgradeManager == null) return;
        for (int i = 0; i < index; i++)
            upgradeManager.UpgradeRodFree();
    }

    private void ApplyBoatIndex(int index)
    {
        if (upgradeManager == null) return;
        for (int i = 0; i < index; i++)
            upgradeManager.UpgradeBoatFree();
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
    }
}
