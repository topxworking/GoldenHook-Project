using UnityEngine;

[CreateAssetMenu(fileName = "WorkerData", menuName = "FishingGame/WorkerData")]
public class WorkerData : ScriptableObject
{
    [Header("Identity")]
    public string crewName;
    public Sprite crewSprite;
    public int level = 1;

    [Header("Stats")]
    public float fishingSpeed = 1f;
    public float incomeBonus = 0.1f;

    [Header("Economy")]
    public int hireCost = 200;
    public int upgradeCost = 300;
    public WorkerData nextUpgrade;

    [Header("Unlock")]
    public bool isUnlocked = false;
}
