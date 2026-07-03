using System;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralForestGenerator : MonoBehaviour
{
    private struct GeneratedForest
    {
        public RectInt cells;
        public int treeCount;

        public GeneratedForest(RectInt cells, int treeCount)
        {
            this.cells = cells;
            this.treeCount = treeCount;
        }
    }

    [Header("References")]
    [SerializeField] private CityGrid grid;
    [SerializeField] private Transform generatedParent;
    [SerializeField] private GameObject[] treePrefabs;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearBeforeGenerate = true;
    [SerializeField] private int seed = 12345;
    [SerializeField] private bool randomizeSeed;

    [Header("Starting Area")]
    [SerializeField] private bool reserveCenteredStartingArea = true;
    [SerializeField] private Vector2Int startingAreaSize = new Vector2Int(18, 18);

    [Header("Sparse Trees")]
    [SerializeField] private int sparseTreeCount = 25;
    [SerializeField] private int edgePaddingCells = 2;
    [SerializeField] private float sparseTreeJitter = 0.35f;
    [SerializeField] private Vector2 sparseScaleRange = new Vector2(0.75f, 1.15f);

    [Header("Dynamic Forests")]
    [SerializeField] private int forestCount = 3;
    [SerializeField] private Vector2Int minForestSize = new Vector2Int(10, 10);
    [SerializeField] private Vector2Int maxForestSize = new Vector2Int(22, 22);
    [Range(0.05f, 1f)]
    [SerializeField] private float forestDensity = 0.45f;
    [SerializeField] private int forestPlacementAttempts = 40;
    [SerializeField] private int minimumForestGapCells = 4;
    [SerializeField] private float forestTreeJitter = 0.45f;
    [SerializeField] private Vector2 forestScaleRange = new Vector2(0.85f, 1.35f);

    [Header("Ground Raycast")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastStartHeight = 50f;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Tree Variation")]
    [SerializeField] private bool randomYRotation = true;

    private readonly List<GameObject> generatedTrees = new List<GameObject>();
    private readonly List<GeneratedForest> generatedForests = new List<GeneratedForest>();

    private void Awake() // Find game objects if not populated
    {
        if (grid == null)
            grid = FindAnyObjectByType<CityGrid>();

        if (generatedParent == null)
            generatedParent = transform;

        if (grid != null && groundMask == 0)
            groundMask = grid.terrainMask;
    }

    private void Start()
    {
        if (generateOnStart)
            Generate();
    }

    [ContextMenu("Generate Forests")]
    public void Generate() // Clears current forest - Initialises Seed (±Random) - Calls Generation Functions
    {
        if (grid == null)
        {
            Debug.LogWarning("ProceduralForestGenerator needs a CityGrid reference.");
            return;
        }

        if (clearBeforeGenerate)
            ClearGeneratedTrees();

        generatedForests.Clear();

        int generationSeed = randomizeSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random random = new System.Random(generationSeed);

        RectInt reservedArea = GetCenteredStartingArea();
        HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();

        GenerateForestRegions(random, reservedArea);
        GenerateForestTrees(random, reservedArea, usedCells);
        GenerateSparseTrees(random, reservedArea, usedCells);
    }

    [ContextMenu("Clear Generated Trees")]
    public void ClearGeneratedTrees() // Every Tree gameobject in the list to be destroyed.
    {
        for (int i = generatedTrees.Count - 1; i >= 0; i--)
        {
            GameObject tree = generatedTrees[i];

            if (tree == null)
                continue;

            if (Application.isPlaying)
                Destroy(tree);
            else
                DestroyImmediate(tree);
        }

        generatedTrees.Clear();
    }

    private void GenerateForestRegions(System.Random random, RectInt reservedArea)
    {
        int attempts = 0;

        while (generatedForests.Count < forestCount && attempts < forestCount * forestPlacementAttempts)
        {
            attempts++;

            RectInt candidate = CreateRandomForestRect(random);

            if (!IsRectInsideGrid(candidate))
                continue;

            if (RectsOverlapWithPadding(candidate, reservedArea, minimumForestGapCells))
                continue;

            if (OverlapsExistingForest(candidate))
                continue;

            int cellArea = candidate.width * candidate.height;
            int treeCount = Mathf.RoundToInt(cellArea * forestDensity);

            generatedForests.Add(new GeneratedForest(candidate, treeCount));
        }
    }

    private RectInt CreateRandomForestRect(System.Random random)
    {
        int width = random.Next(minForestSize.x, maxForestSize.x + 1);
        int height = random.Next(minForestSize.y, maxForestSize.y + 1);

        int maxX = Mathf.Max(0, grid.size.x - width);
        int maxY = Mathf.Max(0, grid.size.y - height);

        int x = random.Next(edgePaddingCells, Mathf.Max(edgePaddingCells + 1, maxX - edgePaddingCells));
        int y = random.Next(edgePaddingCells, Mathf.Max(edgePaddingCells + 1, maxY - edgePaddingCells));

        return new RectInt(x, y, width, height);
    }

    private void GenerateForestTrees(
        System.Random random,
        RectInt reservedArea,
        HashSet<Vector2Int> usedCells
    )
    {
        foreach (GeneratedForest forest in generatedForests)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = forest.treeCount * 30;

            while (placed < forest.treeCount && attempts < maxAttempts)
            {
                attempts++;

                Vector2Int cell = GetRandomCellInRect(forest.cells, random);

                if (!IsCellUsable(cell, reservedArea, usedCells))
                    continue;

                if (!TryGetGroundPosition(cell, forestTreeJitter, random, out Vector3 position))
                    continue;

                GameObject tree = CreateTree(position, forestScaleRange, random, "Dense Forest Tree");

                if (tree == null)
                    continue;

                generatedTrees.Add(tree);
                usedCells.Add(cell);
                placed++;
            }
        }
    }

    private void GenerateSparseTrees(
        System.Random random,
        RectInt reservedArea,
        HashSet<Vector2Int> usedCells
    )
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = sparseTreeCount * 30;

        while (placed < sparseTreeCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2Int cell = GetRandomMapCell(random);

            if (!IsCellUsable(cell, reservedArea, usedCells))
                continue;

            if (IsInsideAnyForest(cell))
                continue;

            if (!TryGetGroundPosition(cell, sparseTreeJitter, random, out Vector3 position))
                continue;

            GameObject tree = CreateTree(position, sparseScaleRange, random, "Sparse Tree");

            if (tree == null)
                continue;

            generatedTrees.Add(tree);
            usedCells.Add(cell);
            placed++;
        }
    }

    private RectInt GetCenteredStartingArea()
    {
        if (!reserveCenteredStartingArea)
            return new RectInt(-99999, -99999, 0, 0);

        int x = Mathf.Max(0, (grid.size.x - startingAreaSize.x) / 2);
        int y = Mathf.Max(0, (grid.size.y - startingAreaSize.y) / 2);

        return new RectInt(
            x,
            y,
            Mathf.Min(startingAreaSize.x, grid.size.x),
            Mathf.Min(startingAreaSize.y, grid.size.y)
        );
    }

    private Vector2Int GetRandomMapCell(System.Random random)
    {
        int minX = Mathf.Clamp(edgePaddingCells, 0, grid.size.x - 1);
        int minY = Mathf.Clamp(edgePaddingCells, 0, grid.size.y - 1);

        int maxX = Mathf.Max(minX + 1, grid.size.x - edgePaddingCells);
        int maxY = Mathf.Max(minY + 1, grid.size.y - edgePaddingCells);

        return new Vector2Int(
            random.Next(minX, maxX),
            random.Next(minY, maxY)
        );
    }

    private Vector2Int GetRandomCellInRect(RectInt rect, System.Random random)
    {
        return new Vector2Int(
            random.Next(rect.xMin, rect.xMax),
            random.Next(rect.yMin, rect.yMax)
        );
    }

    private bool IsCellUsable(Vector2Int cell, RectInt reservedArea, HashSet<Vector2Int> usedCells)
    {
        if (!IsCellInsideGrid(cell))
            return false;

        if (reservedArea.Contains(cell))
            return false;

        if (usedCells.Contains(cell))
            return false;

        return true;
    }

    private bool IsCellInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.y >= 0 &&
               cell.x < grid.size.x &&
               cell.y < grid.size.y;
    }

    private bool IsRectInsideGrid(RectInt rect)
    {
        return rect.xMin >= 0 &&
               rect.yMin >= 0 &&
               rect.xMax <= grid.size.x &&
               rect.yMax <= grid.size.y;
    }

    private bool OverlapsExistingForest(RectInt candidate)
    {
        foreach (GeneratedForest forest in generatedForests)
        {
            if (RectsOverlapWithPadding(candidate, forest.cells, minimumForestGapCells))
                return true;
        }

        return false;
    }

    private bool IsInsideAnyForest(Vector2Int cell)
    {
        foreach (GeneratedForest forest in generatedForests)
        {
            if (forest.cells.Contains(cell))
                return true;
        }

        return false;
    }

    private bool RectsOverlapWithPadding(RectInt a, RectInt b, int padding)
    {
        RectInt paddedA = new RectInt(
            a.xMin - padding,
            a.yMin - padding,
            a.width + padding * 2,
            a.height + padding * 2
        );

        return paddedA.Overlaps(b);
    }

    private bool TryGetGroundPosition(
        Vector2Int cell,
        float jitter,
        System.Random random,
        out Vector3 position
    )
    {
        Vector3 cellCenter = grid.CellToWorld(cell);

        float jitterX = RandomRange(random, -jitter, jitter) * grid.cellSize;
        float jitterZ = RandomRange(random, -jitter, jitter) * grid.cellSize;

        Vector3 jitterOffset = grid.transform.TransformVector(new Vector3(jitterX, 0f, jitterZ));
        Vector3 samplePosition = cellCenter + jitterOffset;

        Vector3 rayStart = samplePosition + Vector3.up * raycastStartHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            position = hit.point;
            return true;
        }

        position = samplePosition;
        return false;
    }

    private GameObject CreateTree(
        Vector3 position,
        Vector2 scaleRange,
        System.Random random,
        string treeName
    )
    {
        GameObject tree;

        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            GameObject prefab = treePrefabs[random.Next(0, treePrefabs.Length)];

            if (prefab == null)
                return null;

            tree = Instantiate(prefab, position, Quaternion.identity, generatedParent);
            tree.name = treeName;
        }
        else
        {
            tree = CreatePlaceholderTree(position, treeName);
            tree.transform.SetParent(generatedParent, true);
        }

        if (randomYRotation)
            tree.transform.rotation = Quaternion.Euler(0f, RandomRange(random, 0f, 360f), 0f);

        float scale = RandomRange(random, scaleRange.x, scaleRange.y);
        tree.transform.localScale = Vector3.one * scale;

        return tree;
    }

    private GameObject CreatePlaceholderTree(Vector3 position, string treeName)
    {
        GameObject root = new GameObject(treeName);
        root.transform.position = position;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(root.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        trunk.transform.localScale = new Vector3(0.16f, 0.45f, 0.16f);

        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.name = "Leaves";
        leaves.transform.SetParent(root.transform, false);
        leaves.transform.localPosition = new Vector3(0f, 1.15f, 0f);
        leaves.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

        return root;
    }

    private float RandomRange(System.Random random, float min, float max)
    {
        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }
}