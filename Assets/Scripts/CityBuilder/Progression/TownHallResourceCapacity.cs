using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TownHallProgression))]
public sealed class TownHallResourceCapacity : MonoBehaviour
{
    [Header("Capacity Per Town Hall Level")]
    [SerializeField]
    private ResourceCapacityLevel[] capacityLevels; // <-- CAPACITIES

    private TownHallProgression progression;

    public event Action CapacitiesChanged;

    private void Awake()
    {
        progression = GetComponent<TownHallProgression>();
    }

    private void OnEnable()
    {
        if (progression == null)
            progression = GetComponent<TownHallProgression>();

        progression.LevelChanged += HandleLevelChanged;
    }

    private void OnDisable()
    {
        if (progression != null)
            progression.LevelChanged -= HandleLevelChanged;
    }

    public int GetCapacity(ResourceType resourceType)
    {
        if (progression == null)
            return 0;

        ResourceCapacityLevel levelData =
            GetLevelData(progression.Level);

        if (levelData == null || levelData.capacities == null)
            return 0;

        foreach (ResourceAmount capacity in levelData.capacities)
        {
            if (capacity.type == resourceType)
                return Mathf.Max(0, capacity.amount);
        }

        return 0;
    }

    private ResourceCapacityLevel GetLevelData(int level)
    {
        if (capacityLevels == null)
            return null;

        foreach (ResourceCapacityLevel levelData in capacityLevels)
        {
            if (
                levelData != null &&
                levelData.townHallLevel == level
            )
            {
                return levelData;
            }
        }

        return null;
    }

    private void HandleLevelChanged(int newLevel)
    {
        CapacitiesChanged?.Invoke();
    }
}