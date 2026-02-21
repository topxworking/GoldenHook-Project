using UnityEngine;

public enum FishRarity { Common, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "FishData", menuName = "FishingGame/FishData")]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    public string fishName;
    public Sprite fishSprite;
    public FishRarity rarity;

    [Header("Stats")]
    public float minWeigth;
    public float maxWeigth;
    public int basePrice;
    public float catchDifficuty = 1f; // 1 = easy, 5 = hard

    [Header("Catch Chance per Zone")]
    public float catchChanceModifier = 1f;

    public int GetSellPrice(float weight)
    {
        float rarityMult = rarity switch
        {
            FishRarity.Common => 1f,
            FishRarity.Rare => 3f,
            FishRarity.Epic => 8f,
            FishRarity.Legendary => 25f,
            _ => 1f
        };
        return Mathf.RoundToInt(basePrice * weight * rarityMult);
    }

    public Color GetRarityColor() => rarity switch
    {
        FishRarity.Common => Color.white,
        FishRarity.Rare => Color.lightSkyBlue,
        FishRarity.Epic => new Color(0.6f, 0.2f, 1f),
        FishRarity.Legendary => Color.yellow,
        _ => Color.white
    };
}
