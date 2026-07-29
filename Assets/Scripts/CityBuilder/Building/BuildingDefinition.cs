using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Medieval City Builder/Building Definition")]
/* 
    Framework defines a building object.
    Right click in PROJECT Window, CREATE, Medieval City Builder, Building Definition.
*/


public class BuildingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea]
    public string description;
    public BuildingCategory category;
    public Sprite icon;

    [Header("Placement")]
    public GameObject prefab;
    public Vector2Int footprint = Vector2Int.one;
    public bool requiresRoadAccess = true;
    public int unlockLevel = 1;

    [Header("Economy")]
    [Min(0)]
    [Tooltip("Maximum number of this building allowed at once. Set to 0 for unlimited.")]
    public int maxInstances = 0;
    public ResourceAmount[] buildCost;
    public ResourceAmount[] storageProvided;

    [FormerlySerializedAs("productionPerTick")]
    public ResourceAmount[] productionPerCycle;

    [Min(1)]
    public int productionIntervalTicks = 1;
    
    public ResourceAmount[] upkeepPerTick;
    public int populationCapacity;
    public int happinessImpact;
    public float buildSeconds = 3f;
}
