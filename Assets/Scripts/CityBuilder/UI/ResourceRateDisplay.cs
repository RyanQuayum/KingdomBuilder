using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceRateDisplay : MonoBehaviour
{
    [Header("Resource")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private bool subtractUpkeep = true;

    [Header("Sources")]
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private EconomyTicker economyTicker;
    [SerializeField] private bool includeExistingSceneBuildings = true;

    [Header("View")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private string rateFormat = "+{0:0}/min";

    private readonly List<BuildingInstance> buildings = new List<BuildingInstance>();

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (buildManager != null)
            buildManager.BuildingPlaced += RegisterBuilding;

        if (includeExistingSceneBuildings)
            RegisterExistingBuildings();

        Refresh();
    }

    private void OnDisable()
    {
        if (buildManager != null)
            buildManager.BuildingPlaced -= RegisterBuilding;
    }

    private void Update()
    {
        Refresh();
    }

    public void Configure(ResourceType newResourceType, BuildManager newBuildManager, EconomyTicker newEconomyTicker)
    {
        if (buildManager != null)
            buildManager.BuildingPlaced -= RegisterBuilding;

        resourceType = newResourceType;
        buildManager = newBuildManager;
        economyTicker = newEconomyTicker;

        if (isActiveAndEnabled && buildManager != null)
            buildManager.BuildingPlaced += RegisterBuilding;

        Refresh();
    }

    private void RegisterExistingBuildings()
    {
        BuildingInstance[] existingBuildings = FindObjectsOfType<BuildingInstance>();

        foreach (BuildingInstance building in existingBuildings)
            RegisterBuilding(building);
    }

    private void RegisterBuilding(BuildingInstance building)
    {
        if (building != null && !buildings.Contains(building))
            buildings.Add(building);

        Refresh();
    }

    private void Refresh()
    {
        if (text == null)
            return;

        text.text = string.Format(rateFormat, CalculatePerMinute());
    }

    private float CalculatePerMinute()
    {
        float tickSeconds = economyTicker != null ? economyTicker.tickSeconds : 60f;

        if (tickSeconds <= 0f)
            return 0f;

        return CalculatePerTick() * (60f / tickSeconds);
    }

    private int CalculatePerTick()
    {
        int amount = 0;

        for (int i = buildings.Count - 1; i >= 0; i--)
        {
            BuildingInstance building = buildings[i];

            if (building == null)
            {
                buildings.RemoveAt(i);
                continue;
            }

            if (!building.IsComplete || building.Definition == null)
                continue;

            amount += SumResource(building.Definition.productionPerTick);

            if (subtractUpkeep)
                amount -= SumResource(building.Definition.upkeepPerTick);
        }

        return amount;
    }

    private int SumResource(ResourceAmount[] resources)
    {
        if (resources == null)
            return 0;

        int amount = 0;

        foreach (ResourceAmount resource in resources)
        {
            if (resource.type == resourceType)
                amount += resource.amount;
        }

        return amount;
    }
}
