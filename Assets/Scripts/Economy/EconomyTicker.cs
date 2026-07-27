using System.Collections.Generic;
using UnityEngine;

public class EconomyTicker : MonoBehaviour
{
    public CityResources resources;
    public float tickSeconds = 1f;
    public ForestBonus forestBonus;

    private readonly List<BuildingInstance> buildings = new List<BuildingInstance>();
    private float timer;
    private double nextTickTime;

    public void Register(BuildingInstance building)
    {
        if (building != null && !buildings.Contains(building))
            buildings.Add(building);
    }

    public void Unregister(BuildingInstance building)
    {
        buildings.Remove(building);
    }

    private void Awake()
    {
        BuildManager buildManager = FindAnyObjectByType<BuildManager>();

        if (buildManager != null)
            buildManager.BuildingPlaced += Register;

        if (forestBonus == null)
            forestBonus = FindAnyObjectByType<ForestBonus>();
    }

    
    private void Start()
    {
        nextTickTime = Time.timeAsDouble + tickSeconds;
    }

    private void Update()
    {
        if (tickSeconds <= 0f)
            return;

        double now = Time.timeAsDouble;
        while (now > nextTickTime)
        {
            Tick();
            nextTickTime += tickSeconds;
        }

    }

    private void ProcessBuilding(BuildingInstance building)
    {
        if (building == null || !building.IsComplete || building.Definition == null) {return;}

        BuildingDefinition definition = building.Definition;

        if (!resources.TrySpend(definition.upkeepPerTick)) // Try Spend actually spends upkeep - Pay this ticks upkeep
            return;
        if (!building.AdvanceProductionTick())
            return;

        resources.Add(definition.productionPerCycle);
    }


    private void Tick()
    {
        if (resources == null)
            return;

        foreach (BuildingInstance building in buildings)
            ProcessBuilding(building);
    }
}
