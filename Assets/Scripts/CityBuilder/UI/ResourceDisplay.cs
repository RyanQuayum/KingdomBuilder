using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [Header("Resource")]
    [SerializeField] private CityResources resources;
    [SerializeField] private ResourceType resourceType;

    [Header("View")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private string amountFormat = "{0:N0}";
    [SerializeField] private bool includeResourceName;
    [SerializeField] private string namedFormat = "{0}: {1}";

    private bool subscribed;

    public ResourceType ResourceType => resourceType;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Configure(CityResources newResources, ResourceType newResourceType)
    {
        Unsubscribe();
        resources = newResources;
        resourceType = newResourceType;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    public void SetResources(CityResources newResources)
    {
        Unsubscribe();
        resources = newResources;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    public void SetResourceType(ResourceType newResourceType)
    {
        resourceType = newResourceType;
        Refresh();
    }

    public void Refresh()
    {
        if (resources == null)
            return;

        UpdateText(resources.Get(resourceType));
    }

    private void Subscribe()
    {
        if (subscribed || resources == null)
            return;

        resources.ResourceChanged += HandleResourceChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || resources == null)
            return;

        resources.ResourceChanged -= HandleResourceChanged;
        subscribed = false;
    }

    private void HandleResourceChanged(ResourceType changedType, int amount)
    {
        if (changedType == resourceType)
            UpdateText(amount);
    }

    private void UpdateText(int amount)
    {
        if (text == null)
            return;

        text.text = FormatAmount(amount);
    }

    protected virtual string FormatAmount(int amount)
    {
        string amountText = string.Format(amountFormat, amount);
        return includeResourceName ? string.Format(namedFormat, resourceType, amountText) : amountText;
    }
}
