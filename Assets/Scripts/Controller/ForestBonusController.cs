using UnityEngine;
public class ForestSystem : MonoBehaviour
{
    [SerializeField] private ProceduralForestGenerator forestGenerator;
    [SerializeField] private int lumberyardBonusRange = 5;

    public bool IsInLumberyardBonusRange(Vector2Int origin, Vector2Int footprint)
    {
        return forestGenerator.IsFootprintInForestBonusRange(
            origin,
            footprint,
            lumberyardBonusRange
        );
    }
}