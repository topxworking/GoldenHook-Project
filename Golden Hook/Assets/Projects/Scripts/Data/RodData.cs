using UnityEngine;

[CreateAssetMenu(fileName = "RodData", menuName = "FishingGame/RodData")]
public class RodData : ScriptableObject
{
    [Header("Identity")]
    public string rodName;
    public Sprite rodSprite;
    public int level = 1;

    [Header("Stats")]
    public float castingSpeed = 1f;
    public float catchRateBonus = 0f;
    public float rarityBonus = 0f;
    public float autoFishInterval = 0f;

    [Header("Economy")]
    public int upgradeCost = 100;
    public RodData nextUpgrade;

    [Header("Unlock")]
    public bool isUnlocked = false;
}
