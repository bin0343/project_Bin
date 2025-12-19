using UnityEngine;

public class ScheduleManager : MonoBehaviour
{
    [Header("초기 설정")]
    public GameObject layerMap;

    public Transform layersParent;

    private void OnEnable()
    {
        ResetToMap();
    }

    public void ResetToMap()
    {
        if (layersParent != null)
        {
            foreach (Transform child in layersParent)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (layerMap != null)
        {
            layerMap.SetActive(true);
        }
    }
}