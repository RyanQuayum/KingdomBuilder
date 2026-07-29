using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BuildingInstance))]
public sealed class TownHallProgression : MonoBehaviour
{
    public const int MinimumLevel = 1;
    public const int MaximumLevel = 10;

    [Header("Town Hall Progression")]
    [SerializeField]
    [Range(MinimumLevel, MaximumLevel)]
    private int level = MinimumLevel;

    public int Level => level;

    private void OnValidate()
    {
        level = Mathf.Clamp(
            level,
            MinimumLevel,
            MaximumLevel
        );
    }
}