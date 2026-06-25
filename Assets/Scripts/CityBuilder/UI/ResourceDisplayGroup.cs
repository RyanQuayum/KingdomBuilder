using UnityEngine;

public class ResourceDisplayGroup : MonoBehaviour
{
    [SerializeField] private CityResources resources;
    [SerializeField] private bool findDisplaysInChildren = true;
    [SerializeField] private ResourceDisplay[] displays;

    private void Awake()
    {
        if (findDisplaysInChildren)
            displays = GetComponentsInChildren<ResourceDisplay>(true);

        ApplyResources();
    }

    public void SetResources(CityResources newResources)
    {
        resources = newResources;
        ApplyResources();
    }

    public void ApplyResources()
    {
        if (displays == null)
            return;

        foreach (ResourceDisplay display in displays)
        {
            if (display != null)
                display.SetResources(resources);
        }
    }
}
