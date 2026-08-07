using System;
using UnityEngine;

[Serializable]
public class ResourceCapacityLevel
{
    [Range(1, 10)]
    public int townHallLevel = 1;

    public ResourceAmount[] capacities;
}