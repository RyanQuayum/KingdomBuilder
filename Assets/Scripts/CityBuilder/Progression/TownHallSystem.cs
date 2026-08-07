using UnityEngine;

public sealed class TownHallSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private CityResources resources;

    public TownHallProgression TownHall { get; private set; }

    private void Awake()
    {
        if (buildManager == null)
            buildManager = FindAnyObjectByType<BuildManager>();

        if (resources == null)
            resources = FindAnyObjectByType<CityResources>();
    }

    private void OnEnable()
    {
        if (buildManager != null)
        {
            buildManager.BuildingPlaced +=
                HandleBuildingPlaced;
        }
    }

    private void OnDisable()
    {
        if (buildManager != null)
        {
            buildManager.BuildingPlaced -=
                HandleBuildingPlaced;
        }
    }

    private void HandleBuildingPlaced(
        BuildingInstance building
    )
    {
        TownHallProgression townHall =
            building.GetComponent<TownHallProgression>();

        if (townHall == null)
            return;

        TownHall = townHall;

        TownHallResourceCapacity capacity =
            building.GetComponent<TownHallResourceCapacity>();

        resources.SetTownHallCapacity(capacity);
    }
}