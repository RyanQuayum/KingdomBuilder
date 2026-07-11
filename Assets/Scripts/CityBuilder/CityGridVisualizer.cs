using UnityEditor.ShaderGraph;
using UnityEngine;

[RequireComponent(typeof(CityGrid))]
public class CityGridVisualizer : MonoBehaviour
{
    [Header("Runtime Grid")]
    public bool showGrid = true;
    public float yOffset = 0.03f;
    public Color lineColor = new Color(1f, 1f, 1f, 0.35f);
    public Material lineMaterial;

    private const string GridLinesName = "Runtime Height Grid Lines";

    private CityGrid grid;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh gridMesh;
    private Material generatedMaterial;

    private void Awake()
    {
        grid = GetComponent<CityGrid>();
    }

    private void Start()
    {
        if (grid != null)
        grid.EnsureInitialized();
        EnsureGridRenderer();
        RebuildGrid();
    }

    private void OnEnable()
    {
        SetGridVisible(showGrid);
    }

    private void OnValidate()
    {
        grid = GetComponent<CityGrid>();

        if (!Application.isPlaying)
            return;

        if (grid != null)
            grid.EnsureInitialized();
    }

    public void RebuildGrid()
    {
        if (grid == null)
            grid = GetComponent<CityGrid>();
        if (grid == null)
            return;
        grid.EnsureInitialized();
        EnsureGridRenderer();

        int cellCount = grid.size.x * grid.size.y;
        int lineCount = cellCount * 4;

        Vector3[] vertices = new Vector3[lineCount * 2];
        int[] indices = new int[vertices.Length];

        int vertexIndex = 0;

        for (int x = 0; x < grid.size.x; x++)
        {
            for (int y = 0; y < grid.size.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                AddCellSquare(cell, vertices, indices, ref vertexIndex);
            }
        }

        if (gridMesh == null)
        {
            gridMesh = new Mesh();
            gridMesh.name = "Height Aware City Grid Lines";
        }

        gridMesh.Clear();
        gridMesh.vertices = vertices;
        gridMesh.SetIndices(indices, MeshTopology.Lines, 0);
        gridMesh.RecalculateBounds();

        meshFilter.sharedMesh = gridMesh;

        ApplyMaterial();
        SetGridVisible(showGrid);
    }

    private void AddCellSquare(
        Vector2Int cell,
        Vector3[] vertices,
        int[] indices,
        ref int vertexIndex
    )
    {
        float xMin = cell.x * grid.cellSize;
        float xMax = (cell.x + 1) * grid.cellSize;
        float zMin = cell.y * grid.cellSize;
        float zMax = (cell.y + 1) * grid.cellSize;

        float y = grid.GetWorldHeight(cell) + yOffset;

        Vector3 bottomLeft = new Vector3(xMin, y, zMin);
        Vector3 bottomRight = new Vector3(xMax, y, zMin);
        Vector3 topRight = new Vector3(xMax, y, zMax);
        Vector3 topLeft = new Vector3(xMin, y, zMax);

        AddLine(bottomLeft, bottomRight, vertices, indices, ref vertexIndex);
        AddLine(bottomRight, topRight, vertices, indices, ref vertexIndex);
        AddLine(topRight, topLeft, vertices, indices, ref vertexIndex);
        AddLine(topLeft, bottomLeft, vertices, indices, ref vertexIndex);
    }

    private void AddLine(
        Vector3 start,
        Vector3 end,
        Vector3[] vertices,
        int[] indices,
        ref int vertexIndex
    )
    {
        vertices[vertexIndex] = start;
        indices[vertexIndex] = vertexIndex;
        vertexIndex++;

        vertices[vertexIndex] = end;
        indices[vertexIndex] = vertexIndex;
        vertexIndex++;
    }

    private void EnsureGridRenderer()
    {
        Transform existing = transform.Find(GridLinesName);

        GameObject gridLines = existing != null
            ? existing.gameObject
            : new GameObject(GridLinesName);

        gridLines.transform.SetParent(transform, false);
        gridLines.transform.localPosition = Vector3.zero;
        gridLines.transform.localRotation = Quaternion.identity;
        gridLines.transform.localScale = Vector3.one;

        meshFilter = gridLines.GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gridLines.AddComponent<MeshFilter>();

        meshRenderer = gridLines.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            meshRenderer = gridLines.AddComponent<MeshRenderer>();
    }

    private void ApplyMaterial()
    {
        if (lineMaterial != null)
        {
            meshRenderer.sharedMaterial = lineMaterial;
            return;
        }

        if (generatedMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find("Hidden/Internal-Colored");

            generatedMaterial = new Material(shader);
            generatedMaterial.name = "Generated Height Grid Material";
        }

        generatedMaterial.color = lineColor;
        meshRenderer.sharedMaterial = generatedMaterial;
    }

    private void SetGridVisible(bool visible)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = visible;
    }

    private void OnDestroy()
    {
        if (gridMesh != null)
            Destroy(gridMesh);

        if (generatedMaterial != null)
            Destroy(generatedMaterial);
    }
}