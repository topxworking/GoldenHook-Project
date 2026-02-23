using System;
using System.Collections.Generic;
using UnityEngine;

public struct FishCaughtEvent
{
    public FishData fishData;
    public float weight;
    public int sellPrice;
}

public struct MoneyChangedEvent
{
    public int OldAmount;
    public int NewAmount;
    public int Delta;
}

public struct UpgradeEvent
{
    public string UpgradeType;
    public int NewLevel;
}

public struct ZoneUnlockedEvent
{
    public SeaZoneData ZoneData;
}

public static class EventManager
{
    private static readonly Dictionary<Type, List<Delegate>> _listeners = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        var key = typeof(T);
        if (!_listeners.ContainsKey(key))
            _listeners[key] = new List<Delegate>();
        _listeners[key].Add(callback);
    }

    public static void UnSubscribe<T>(Action<T> callback)
    {
        var key = typeof(T);
        if (_listeners.ContainsKey(key))
            _listeners[key].Remove(callback);
    }

    public static void Publish<T>(T eventData)
    {
        var key = typeof (T);
        if (!_listeners.TryGetValue(key, out var callbacks)) return;

        foreach (var d in callbacks.ToArray())
            (d as Action<T>)?.Invoke(eventData);
    }

    public static void Clear() => _listeners.Clear();
}
