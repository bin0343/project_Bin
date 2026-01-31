using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeMap : MonoBehaviour
{
    private MapController mapController;

    private void Start()
    {
        GameObject canvas = GameObject.Find("UI_Canvas");

        if (canvas != null)
        {
            Transform panelTrans = canvas.transform.Find("WorldMapPanel");

            if (panelTrans != null)
            {
                mapController = panelTrans.GetComponent<MapController>();
            }
            else
            {
                Debug.LogError("Canvas 밑에 'MapPanel'이라는 이름의 오브젝트가 없습니다!");
            }
        }
        else
        {
            Debug.LogError("현재 씬에서 'Canvas'를 찾을 수 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && mapController != null)
        {
            //mapController.OpenMapPanel();
        }
    }
}
