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
            // 2. transform.Find는 꺼져있는 자식 오브젝트도 찾을 수 있습니다!
            // (주의: 하이어라키에 있는 패널 이름이 "MapPanel"과 정확히 일치해야 합니다)
            Transform panelTrans = canvas.transform.Find("MapWindow");

            if (panelTrans != null)
            {
                // 3. 찾은 패널에서 스크립트 가져오기
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
            mapController.OpenMapPanel();
        }
    }
}
