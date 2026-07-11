using UnityEngine;

public class CityGrid : MonoBehaviour
{
    [Header("Grid")]
    public Vector2Int size = new Vector2Int(64, 64);
    public float cellSize = 1f;
    public LayerMask terrainMask = ~0;

    [Header("Height")]
    public float levelHeight = 0.5f;
    public int defaultHeightLevel = 0;

    private BuildingInstance[,] occupied;
    /*
        2D array representing the grid, either occupied or NULL, e.g:
        occupied[0,0]
        ...
        occupied[64,64]
    */
    private int[,] heightLevels; // 2D array with each cell storing cell height e.g. 1..2..3..4...

    public void EnsureInitialized()
{
    size = new Vector2Int(
        Mathf.Max(1, size.x),
        Mathf.Max(1, size.y)
    );

    if (occupied == null ||
        occupied.GetLength(0) != size.x ||
        occupied.GetLength(1) != size.y)
    {
        occupied = new BuildingInstance[size.x, size.y];
    }

    if (heightLevels == null ||
        heightLevels.GetLength(0) != size.x ||
        heightLevels.GetLength(1) != size.y)
    {
        int[,] oldHeights = heightLevels;
        heightLevels = new int[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (oldHeights != null &&
                    x < oldHeights.GetLength(0) &&
                    y < oldHeights.GetLength(1))
                {
                    heightLevels[x, y] = oldHeights[x, y];
                }
                else
                {
                    heightLevels[x, y] = defaultHeightLevel;
                }
            }
        }
    }
}

    private void Awake()
    {
        EnsureInitialized();
        occupied = new BuildingInstance[size.x, size.y]; // Clears Grid on start
        heightLevels = new int[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                heightLevels[x, y] = defaultHeightLevel; // Set grid to default height
            }
        }
    }

    private void OnValidate()
    {
        size = new Vector2Int(
            Mathf.Max(1, size.x),
            Mathf.Max(1, size.y)
        );

        if (Application.isPlaying)
            EnsureInitialized();
    }

    public bool TryGetCellFromWorld(Vector3 worldPosition, out Vector2Int cell)
    /*
        Converts world location to closest matching cell position.
    */
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        cell = new Vector2Int(Mathf.FloorToInt(local.x / cellSize), Mathf.FloorToInt(local.z / cellSize));
        return IsInBounds(cell);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    /*
        Converts cell to world location.
    */
    {
        EnsureInitialized();
        Vector3 local = new Vector3((cell.x + 0.5f) * cellSize, GetWorldHeight(cell), (cell.y + 0.5f) * cellSize);
        return transform.TransformPoint(local);
    }

    // Height getter/setters.
    public int GetHeightLevel(Vector2Int cell)
    {
        EnsureInitialized();
        if (!IsInBounds(cell))
            return defaultHeightLevel;

        return heightLevels[cell.x, cell.y];
    }

        public void SetHeightLevel(Vector2Int cell, int heightLevel)
    {
        if (!IsInBounds(cell))
            return;

        heightLevels[cell.x, cell.y] = heightLevel;
    }

        public float GetWorldHeight(Vector2Int cell)
    {
        return GetHeightLevel(cell) * levelHeight;
    }

    public bool CanPlace(Vector2Int origin, Vector2Int footprint)
    /*
        Checks if every cell of the potential building is in the grid
        And that none of those cells are occupied + incorrect height.
    */
    {
        if (!IsInBounds(origin))
            return false;
        int baseHeight = GetHeightLevel(origin);

        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);

                if (!IsInBounds(cell) || occupied[cell.x, cell.y] != null)
                    return false;

                if (GetHeightLevel(cell) != baseHeight)
                    return false;
            }
        }

        return true;
    }

    public void Occupy(BuildingInstance building)
    // Marks cells in the grid as occupied
    {
        SetCells(building, building.Origin, building.Definition.footprint);
    }

    public void Clear(BuildingInstance building)
    // Clears cells in the grid, making them NULL.
    {
        SetCells(null, building.Origin, building.Definition.footprint);
    }

    private void SetCells(BuildingInstance building, Vector2Int origin, Vector2Int footprint)
    {
        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);

                if (IsInBounds(cell))
                    occupied[cell.x, cell.y] = building;
            }
        }
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < size.x && cell.y < size.y;
    }
}
