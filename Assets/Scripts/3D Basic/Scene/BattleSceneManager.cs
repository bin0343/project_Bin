using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : MonoBehaviour
{
    public Transform combatCamera; // 쿼터뷰 카메라

    void Start()
    {
        Player_Move player = FindObjectOfType<Player_Move>();
        if (player != null)
        {
            // 전투 씬 시작하자마자 이동 기준을 쿼터뷰 카메라로 변경
            player.SetReferenceTransform(combatCamera);

            // 마우스 커서 보이게 설정
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
