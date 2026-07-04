using System.Collections.Generic;
using UnityEngine;

public class ForestBonusRangePreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CityGrid grid;
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private ProceduralForestGenerator forestGenerator;
    [SerializeField] private ForestBonus forestBonus;

    [Header("Lumberyard Match")]
    [SerializeField] private string lumberyardId = "lumberyard";
    [SerializeField] private string lumberyardDisplayName = "Lumberyard";

    [Header("Preview")]
    [SerializeField] private bool showOnlyWhenPlacingLumberyard = true;
    [SerializeField] private Material bonusRangeMaterial;
    [SerializeField] private float yOffset = 0.035f;
    [SerializeField] private float cellScale = 0.9f;

    private readonly List<GameObject> markers = new List<GameObject>();
    private HashSet<Vector2Int> cachedCells = new HashSet<Vector2Int>();
    private bool currentlyVisible;

    private void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<CityGrid>();

        if (buildManager == null)
            buildManager = FindAnyObjectByType<BuildManager>();

        if (forestGenerator == null)
            forestGenerator = FindAnyObjectByType<ProceduralForestGenerator>();

        if (forestBonus == null)
            forestBonus = FindAnyObjectByType<ForestBonus>();
    }

    private void Update()
    {
        bool shouldShow = ShouldShowPreview();

        if (!shouldShow)
        {
            if (currentlyVisible)
                HideMarkers();

            currentlyVisible = false;
            return;
        }

        if (!currentlyVisible)
        {
            RebuildPreview();
            currentlyVisible = true;
        }
    }

    [ContextMenu("Rebuild Forest Bonus Preview")]
    public void RebuildPreview()
    {
        if (grid == null || forestGenerator == null || forestBonus == null)
            return;

        cachedCells = forestGenerator.GetForestBonusCells(forestBonus.BonusRangeCells);

        while (markers.Count < cachedCells.Count)
            markers.Add(CreateMarker());

        int markerIndex = 0;

        foreach (Vector2Int cell in cachedCells)
        {
            GameObject marker = markers[markerIndex];
            marker.SetActive(true);

            Vector3 worldPosition = grid.CellToWorld(cell);
            worldPosition.y += yOffset;

            marker.transform.position = worldPosition;
            marker.transform.localScale = new Vector3(
                grid.cellSize * cellScale,
                0.02f,
                grid.cellSize * cellScale
            );

            Renderer markerRenderer = marker.GetComponent<Renderer>();

            if (markerRenderer != null && bonusRangeMaterial != null)
                markerRenderer.sharedMaterial = bonusRangeMaterial;

            markerIndex++;
        }

        for (int i = markerIndex; i < markers.Count; i++)
            markers[i].SetActive(false);
    }

    public void HideMarkers()
    {
        for (int i = 0; i < markers.Count; i++)
        {
            if (markers[i] != null)
                markers[i].SetActive(false);
        }
    }

    private bool ShouldShowPreview()
    {
        if (grid == null || forestGenerator == null || forestBonus == null)
            return false;

        if (!forestGenerator.HasGeneratedForests())
            return false;

        if (!showOnlyWhenPlacingLumberyard)
            return true;

        if (buildManager == null || buildManager.selectedBuilding == null)
            return false;

        return IsLumberyard(buildManager.selectedBuilding);
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

    private GameObject CreateMarker()
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Forest Bonus Range Cell";
        marker.transform.SetParent(transform, true);

        Collider markerCollider = marker.GetComponent<Collider>();

        if (markerCollider != null)
            Destroy(markerCollider);

        return marker;
    }
}