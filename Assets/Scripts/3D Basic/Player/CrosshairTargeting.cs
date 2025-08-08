using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairTargeting : MonoBehaviour
{
    public Camera playerCamera;              // 플레이어가 보는 카메라 (Inspector에서 할당)
    public float maxDistance = 20f;          // 레이캐스트 최대 거리
    public UI_MonsterUIManager MonsterUIManager;  // 몬스터 UI 매니저 (Inspector에서 할당)

    private void Start()
    {
        MonsterUIManager = FindObjectOfType<UI_MonsterUIManager>();
    }
    void Update()
    {
        // 화면 가운데 점 좌표 (크로스헤어 위치)
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // 몬스터를 맞췄으면 타겟 설정
                GameObject target = hit.collider.gameObject;
                MonsterUIManager.SetTarget(target);
            }
            else
            {
                // 몬스터 아닌 걸 맞췄으면 타겟 해제
                MonsterUIManager.ClearTarget();
            }
        }
        else
        {
            // 아무것도 맞지 않으면 타겟 해제
            MonsterUIManager.ClearTarget();
        }
    }
}
