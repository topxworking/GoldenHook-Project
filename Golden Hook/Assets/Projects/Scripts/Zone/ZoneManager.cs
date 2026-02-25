using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [SerializeField] private List<SeaZoneData> allZones = new();
    [SerializeField] private FishingController fishingController;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    private readonly HashSet<int> _unlockedZoneIndexes = new();
    public SeaZoneData CurrentZone { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _unlockedZoneIndexes.Clear();
        _unlockedZoneIndexes.Add(0);

        foreach (var zone in allZones)
        {
            if (zone.isUnlocked)
                _unlockedZoneIndexes.Add(zone.zoneIndex);
        }

        foreach (var zone in allZones)
        {
            zone.isUnlocked = _unlockedZoneIndexes.Contains(zone.zoneIndex);
        }
    }

    private void Start()
    {
        var startZone = allZones.Find(z => z.zoneIndex == 0);
        if (startZone != null) SwitchToZone(startZone);

        EventManager.Publish(new ZoneUnlockedEvent { ZoneData = startZone });
    }

    public bool IsZoneUnlocked(SeaZoneData zone) => _unlockedZoneIndexes.Contains(zone.zoneIndex);
    public bool IsZoneUnlocked(int index) => _unlockedZoneIndexes.Contains(index);

    public bool TryUnlockZone(SeaZoneData zone)
    {
        if (zone == null) return false;
        if (IsZoneUnlocked(zone)) return false;

        if (zone.requiredZoneIndex >= 0 && !IsZoneUnlocked(zone.requiredZoneIndex))
        {
            return false;
        }

        if (!EconomyManager.Instance.TrySpend(zone.unlockCost))
        {
            return false;
        }

        _unlockedZoneIndexes.Add(zone.zoneIndex);
        zone.isUnlocked = true;

        EventManager.Publish(new ZoneUnlockedEvent { ZoneData = zone });
        return true;
    }

    public void SwitchToZone(SeaZoneData zone)
    {
        if (!IsZoneUnlocked(zone)) return;

        CurrentZone = zone;
        fishingController?.SetZone(zone);
        if (backgroundRenderer != null) backgroundRenderer.sprite = zone.zoneBackground;
        Camera.main.backgroundColor = zone.zoneThemeColor * 0.3f;

        UpgradeManager.Instance?.RecalculatePassiveIncome();
    }

    public List<SeaZoneData> GetAllZones() => allZones;
    public List<int> GetUnlockedIndexes() => new(_unlockedZoneIndexes);

    public void LoadUnlockedZones(List<int> indexes)
    {
        foreach (var zone in allZones)
            zone.isUnlocked = false;

        _unlockedZoneIndexes.Clear();
        _unlockedZoneIndexes.Add(0);

        foreach (int i in indexes)
            _unlockedZoneIndexes.Add(i);

        foreach (var zone in allZones)
            if (_unlockedZoneIndexes.Contains(zone.zoneIndex))
                zone.isUnlocked = true;
    }

    public void ResetUnlockedZones()
    {
        _unlockedZoneIndexes.Clear();
        _unlockedZoneIndexes.Add(0);

        foreach (var zone in allZones)
            zone.isUnlocked = zone.zoneIndex == 0;

        var startZone = allZones.Find(z => z.zoneIndex == 0);
        if (startZone != null) SwitchToZone(startZone);

        EventManager.Publish(new ZoneUnlockedEvent { ZoneData = startZone });
    }
}