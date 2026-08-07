using System;
using System.Collections.Generic;
using UnityEngine;

public class CityResources : MonoBehaviour
{
    [SerializeField] private ResourceAmount[] startingResources =
    {
        new ResourceAmount(ResourceType.Gold, 250),
        new ResourceAmount(ResourceType.Wood, 100),
        new ResourceAmount(ResourceType.Stone, 50),
        new ResourceAmount(ResourceType.Food, 75)
    };

    private readonly Dictionary<ResourceType, int> amounts = new Dictionary<ResourceType, int>();

    public event Action<ResourceType, int> ResourceChanged;

    [SerializeField]
    private TownHallResourceCapacity townHallCapacity;

    public int Get(ResourceType type)
    {
        return amounts.TryGetValue(type, out int amount) ? amount : 0;
    }

    public bool CanAfford(ResourceAmount[] cost)
    {
        if (cost == null)
            return true;

        foreach (ResourceAmount resource in cost)
        {
            if (Get(resource.type) < resource.amount)
                return false;
        }

        return true;
    }

    public bool TrySpend(ResourceAmount[] cost)
    {
        if (!CanAfford(cost))
            return false;

        Add(cost, -1);
        return true;
    }

    public void Add(ResourceAmount[] resources, int multiplier = 1)
    {
        if (resources == null)
            return;

        foreach (ResourceAmount resource in resources)
            Add(resource.type, resource.amount * multiplier);
    }

    public void Add(ResourceType type, int amount)
    {
        int currentAmount = Get(type);
        int newAmount = Mathf.Max(0, Get(type) + amount);
        if (UsesStorageCapacity(type))
        {
            int capacity = GetCapacity(type);
            newAmount = Mathf.Min(newAmount, capacity);
        }

        if (newAmount == currentAmount)
            return;
        
        amounts[type] = newAmount;
        ResourceChanged?.Invoke(type, newAmount);
    }

    private void Awake()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            amounts[type] = 0;

        Add(startingResources);
    }

    private bool UsesStorageCapacity(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold:
            case ResourceType.Wood:
            case ResourceType.Stone:
            case ResourceType.Food:
            case ResourceType.Tools:
                return true;

            default:
                return false;
        }
    }

    public int GetCapacity(ResourceType type)
    {
        if (!UsesStorageCapacity(type))
            return int.MaxValue;

        if (townHallCapacity == null)
            return 0;

        return townHallCapacity.GetCapacity(type);
    }

    public void SetTownHallCapacity(TownHallResourceCapacity capacity)
    {
        if (townHallCapacity != null)
        {
            townHallCapacity.CapacitiesChanged -=
                HandleCapacitiesChanged;
        }

        townHallCapacity = capacity;

        if (townHallCapacity != null)
        {
            townHallCapacity.CapacitiesChanged +=
                HandleCapacitiesChanged;
        }

        ClampAllResourcesToCapacity();
    }

    private void HandleCapacitiesChanged()
    {
        ClampAllResourcesToCapacity();
    }

    private void ClampAllResourcesToCapacity()
    {
        foreach (ResourceType type in Enum.GetValues(
            typeof(ResourceType)
        ))
        {
            if (!UsesStorageCapacity(type))
                continue;

            int currentAmount = Get(type);
            int capacity = GetCapacity(type);
            int clampedAmount = Mathf.Min(
                currentAmount,
                capacity
            );

            if (clampedAmount == currentAmount)
                continue;

            amounts[type] = clampedAmount;
            ResourceChanged?.Invoke(type, clampedAmount);
        }
    }
}
