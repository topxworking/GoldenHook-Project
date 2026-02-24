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
        _unlockedZoneIndexes.Add(0);
    }

    private void Start()
    {
        var startZone = allZones.Find(z => z.zoneIndex == 0);
        if (startZone != null) SwitchToZone(startZone);
    }

    public bool IsZoneUnlocked(SeaZoneData zone) => _unlockedZoneIndexes.Contains(zone.zoneIndex);
    public bool IsZoneUnlocked(int index) => _unlockedZoneIndexes.Contains(index);

    public bool TryUnlockZone(SeaZoneData zone)
    {
        if (zone == null) return false;
        if (IsZoneUnlocked(zone)) return false;

        if (zone.requiredZoneIndex >= 0 && !IsZoneUnlocked(zone.requiredZoneIndex))
        {
            Debug.Log($"[Zone] Need to unlock zone {zone.requiredZoneIndex} first");
            return false;
        }

        if (!EconomyManager.Instance.TrySpend(zone.unlockCost))
        {
            Debug.Log($"[Zone] Not enough money");
            return false;
        }

        _unlockedZoneIndexes.Add(zone.zoneIndex);
        EventManager.Publish(new ZoneUnlockedEvent { ZoneData = zone });
        Debug.Log($"[Zone] Unlocked: {zone.zoneName}");
        return true;
    }

    public void SwitchToZone(SeaZoneData zone)
    {
        if (!IsZoneUnlocked(zone)) return;
        CurrentZone = zone;
        fishingController?.SetZone(zone);
        if (backgroundRenderer != null) backgroundRenderer.sprite = zone.zoneBackground;
        Camera.main.backgroundColor = zone.zoneThemeColor * 0.3f;
        Debug.Log($"[Zone] Switched to: {zone.zoneName}");
    }

    public List<SeaZoneData> GetAllZones() => allZones;
    public List<int> GetUnlockedIndexes() => new(_unlockedZoneIndexes);

    public void LoadUnlockedZones(List<int> indexes)
    {
        _unlockedZoneIndexes.Clear();
        _unlockedZoneIndexes.Add(0);
        foreach (int i in indexes)
            _unlockedZoneIndexes.Add(i);
    }
}