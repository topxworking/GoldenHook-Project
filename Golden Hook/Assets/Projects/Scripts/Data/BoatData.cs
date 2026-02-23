using UnityEngine;

[CreateAssetMenu(fileName = "BoatData", menuName = "FishingGame/BoatData")]
public class BoatData : ScriptableObject
{
    [Header("Identity")]
    public string boatName;
    public Sprite boatSprite;
    public int level = 1;

    [Header("Stats")]
    public int workerSlots = 1;
    public float incomeMultiplier = 1f;
    public float storageCapacity = 20f;

    [Header("Economy")]
    public int upgradeCost = 500;
    public BoatData nextUpgrade;

    [Header("Unlock")]
    public bool isUnlocked = false;
}
