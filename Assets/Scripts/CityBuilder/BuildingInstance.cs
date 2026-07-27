using UnityEngine;

public class BuildingInstance : MonoBehaviour
/*
    Every Building is a building Instance, with these behaviours/atts.
*/

{
    public BuildingDefinition Definition { get; private set; }
    public Vector2Int Origin { get; private set; }
    public bool IsComplete { get; private set; }
    private int productionTicksElapsed;
    private float buildTimer;

    public float BuildProgress01 =>
        Definition == null || Definition.buildSeconds <= 0f
        ? 1f 
        : Mathf.Clamp01(buildTimer / Definition.buildSeconds);
    /* 
        Represents Progress from NOT Built to Built as a float e.g: 0.0 -> 1.0
    */
    public int ProductionIntervalTicks =>
        Definition == null
        ? 1
        : Mathf.Max(1, Definition.productionIntervalTicks);

    public float ProductionProgress01 =>
        Mathf.Clamp01(
            productionTicksElapsed / (float)ProductionIntervalTicks
        );

    public bool AdvanceProductionTick()
    {
        if (!IsComplete || Definition == null)
            return false;

        int interval = Mathf.Max(1, Definition.productionIntervalTicks);

        productionTicksElapsed++;

        if (productionTicksElapsed < interval)
            return false;

        productionTicksElapsed -= interval;
        return true;
    }

    public void Initialize(BuildingDefinition definition, Vector2Int origin)
    {
        Definition = definition;
        Origin = origin;
        productionTicksElapsed = 0;
        IsComplete = definition.buildSeconds <= 0f;
        buildTimer = IsComplete ? definition.buildSeconds : 0f;
        name = definition.displayName + " (" + origin.x + "," + origin.y + ")";
    }

    private void Update()
    {
        if (IsComplete || Definition == null)
            return;

        buildTimer += Time.deltaTime;

        if (buildTimer >= Definition.buildSeconds)
            IsComplete = true;
    }
}
