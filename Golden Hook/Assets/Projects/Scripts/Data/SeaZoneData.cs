using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FishSpawnEntry
{
    public FishData fishData;
    [Range(0f, 1f)]
    public float spawnWeightOveride = 1f;
}

[CreateAssetMenu(fileName = "SeaZoneData", menuName = "FishingGame/SeaZoneData")]
public class SeaZoneData : ScriptableObject
{
    [Header("Identity")]
    public string zoneName;
    public Sprite zoneBackground;
    public Color zoneThemeColor = Color.blue;
    public int ZoneIndex = 0;

    [Header("Fish Pool")]
    public List<FishSpawnEntry> fishPool = new();

    [Header("Unlock")]
    public bool isUnlocked = false;
    public int unlockCost = 0;
    public int requiredZoneIndex = -1;

    [Header("Zone Modifiers")]
    public float rareFishBonus = 0f;
    public float incomeMultipier = 1f;
}
