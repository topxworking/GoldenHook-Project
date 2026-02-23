using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class FishFactory
{
    private static readonly Dictionary<FishRarity, float> _rarityWeights = new()
    {
        { FishRarity.Common, 60f },
        { FishRarity.Rare, 30f },
        { FishRarity.Epic, 9f },
        { FishRarity.Legendary, 1f },
    };

    public static CaughtFish CreateFish(SeaZoneData zone, RodData rod)
    {
        var fishData = RollFishData(zone, rod);
        if (fishData == null) return null;

        float weight = Random.Range(fishData.minWeight, fishData.maxWeight);
        int price = fishData.GetSellPrice(weight);

        return new CaughtFish(fishData, weight, price);
    }

    private static FishData RollFishData(SeaZoneData zone, RodData rod)
    {
        if (zone.fishPool == null || zone.fishPool.Count == 0)
        {
            return null;
        }

        FishRarity rolledRarity = RollRarity(zone.rareFishBonus + (rod?.rarityBonus ?? 0f));

        var candidates = zone.fishPool
            .Where(e => e.fishData.rarity == rolledRarity)
            .ToList();

        if (candidates.Count == 0)
            candidates = zone.fishPool.Where(e => e.fishData.rarity == FishRarity.Common).ToList();

        if (candidates.Count == 0) return null;

        float total = candidates.Sum(e => e.spawnWeightOveride > 0 ? e.spawnWeightOveride : 1f);
        float roll = Random.Range(0f, total);
        float acc = 0f;

        foreach (var entry in candidates)
        {
            float w = entry.spawnWeightOveride > 0 ? entry.spawnWeightOveride : 1f;
            acc += w;
            if (roll <= acc) return entry.fishData;
        }

        return candidates[^1].fishData;
    }

    private static FishRarity RollRarity(float rarityBonus)
    {
        float commonW = Mathf.Max(1f, _rarityWeights[FishRarity.Common] - rarityBonus * 100f);
        float rareW = _rarityWeights[FishRarity.Rare] + rarityBonus * 60f;
        float epicW = _rarityWeights[FishRarity.Epic] + rarityBonus * 30f;
        float legendaryW = _rarityWeights[FishRarity.Legendary] + rarityBonus * 10f;

        float total = commonW + rareW + epicW + legendaryW;
        float roll = Random.Range(0f, total);

        if (roll < commonW) return FishRarity.Common;
        if (roll < commonW + rareW) return FishRarity.Rare;
        if (roll < commonW + rareW + epicW) return FishRarity.Epic;
        return FishRarity.Legendary;
    }
}

public class CaughtFish
{
    public FishData Data { get; }
    public float Weight { get; }
    public int SellPrice { get; }

    public CaughtFish(FishData data, float weight, int sellPrice)
    {
        Data = data;
        Weight = weight;
        SellPrice = sellPrice;
    }
}
