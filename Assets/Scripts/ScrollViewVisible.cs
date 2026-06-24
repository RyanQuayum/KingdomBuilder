using UnityEngine;
using UnityEngine.UI;
public class ScrollViewVisable : MonoBehaviour
{
    public GameObject scrollViewObject;

    public void EnableScroll()
    {
        scrollViewObject.SetActive(true);
    }

    public void DisableScroll()
    {
        scrollViewObject.SetActive(false);
    }
}
