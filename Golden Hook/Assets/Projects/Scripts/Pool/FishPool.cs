using System.Collections.Generic;
using UnityEngine;

public class FishPool : MonoBehaviour
{
    public static FishPool Instance { get; private set; }

    [SerializeField] private FishController fishPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private readonly Queue<FishController> _pool = new();

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < initialPoolSize; i++)
            CreatePooledFish();
    }

    private FishController CreatePooledFish()
    {
        var fish = Instantiate(fishPrefab, transform);
        fish.gameObject.SetActive(false);
        fish.OnReturnToPool += ReturnFish;
        _pool.Enqueue(fish);
        return fish;
    }

    public FishController Get(FishData date, Vector3 spawnPos)
    {
        if (_pool.Count == 0) CreatePooledFish();

        var fish = _pool.Dequeue();
        fish.transform.position = spawnPos;
        fish.gameObject.SetActive(true);
        return fish;
    }

    private void ReturnFish(FishController fish)
    {
        fish.gameObject.SetActive(false);
        _pool.Enqueue(fish);
    }
}
