using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private ProceduralForestGenerator forestGenerator;
    // [SerializeField] private ProceduralStoneGenerator stoneGenerator;
    void Start()
    {
        generateWorld();
    }

    public void generateWorld()
    {
        forestGenerator.Generate();
    }
}
