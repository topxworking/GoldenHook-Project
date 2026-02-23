using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [Header("All Zone")]
    [SerializeField] private List<SeaZoneData> allZones = new();

    [Header("References")]
    [SerializeField] private FishingController fishingController;
    [SerializeField] private SpriteRenderer backgroindRenderer;

    public SeaZoneData CurrentZone { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        foreach (var zone in allZones)
        {
            if (zone.isUnlocked)
            {
                SwitchToZone(zone);
                return;
            }
        }

        if (allZones.Count > 0)
        {
            allZones[0].isUnlocked = true;
            SwitchToZone(allZones[0]);
        }
    }

    public bool TryUnlockZone(SeaZoneData zone)
    {
        if (zone.isUnlocked) return false;

        if (zone.reqiredZoneIndex >= 0)
        {
            var prereq = allZones.Find(z => z.ZoneIndex == zone.reqiredZoneIndex);
            if (prereq != null && !prereq.isUnlocked)
            {
                Debug.Log($"[Zone] Must unlock zone '{prereq.zoneName}' first.");
                return false;
            }
        }

        if (!EconomyManager.Instance.TrySpend(zone.unlockCost)) return false;

        zone.isUnlocked = true;
        EventManager.Publish(new ZoneUnlockedEvent { ZoneData = zone });
        Debug.Log($"[Zone] Unlocked: {zone.zoneName}");
        return true;
    }

    public void SwitchToZone(SeaZoneData zone)
    {
        if (!zone.isUnlocked) return;
        CurrentZone = zone;
        fishingController?.SetZone(zone);

        if (backgroindRenderer != null)
            backgroindRenderer.sprite = zone.zoneBackground;

        Camera.main.backgroundColor = zone.zoneThemeColor * 0.3f;
        Debug.Log($"[Zone] Switched to: {zone.zoneName}");
    }

    public List<SeaZoneData> GetAllZones() => allZones;
}
