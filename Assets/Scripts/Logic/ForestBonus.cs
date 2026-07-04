using UnityEngine;

public class ForestBonus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralForestGenerator forestGenerator;

    [Header("Lumberyard Match")]
    [SerializeField] private string lumberyardId = "lumberyard";
    [SerializeField] private string lumberyardDisplayName = "Lumberyard";

    [Header("Bonus")]
    [SerializeField] private int bonusRangeCells = 5;
    [SerializeField] private ResourceType bonusResource = ResourceType.Wood;
    [SerializeField] private int bonusAmountPerTick = 5;

    public int BonusRangeCells => bonusRangeCells;

    private void Awake()
    {
        if (forestGenerator == null)
            forestGenerator = FindAnyObjectByType<ProceduralForestGenerator>(); // Find Forest gen script
    }

    public bool HasBonus(BuildingInstance building)
    {
        if (building == null || building.Definition == null)
            return false;

        if (forestGenerator == null || !forestGenerator.HasGeneratedForests())
            return false;

        if (!IsLumberyard(building.Definition))
            return false;

        return forestGenerator.IsFootprintInForestBonusRange(
            building.Origin,
            building.Definition.footprint,
            bonusRangeCells
        );
    }

    public void ApplyBonus(BuildingInstance building, CityResources resources)
    {
        if (resources == null)
            return;

        if (!HasBonus(building))
            return;

        resources.Add(bonusResource, bonusAmountPerTick);
    }

    private bool IsLumberyard(BuildingDefinition definition)
    {
        if (definition == null)
            return false;

        if (!string.IsNullOrWhiteSpace(lumberyardId) && definition.id == lumberyardId)
            return true;

        if (!string.IsNullOrWhiteSpace(lumberyardDisplayName) && definition.displayName == lumberyardDisplayName)
            return true;

        return false;
    }
}