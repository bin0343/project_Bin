using UnityEngine;

public class MapInteractionTrigger : MonoBehaviour
{
    // OnTriggerEnter: Trigger 체크된 콜라이더에 닿았을 때 실행
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어인지 태그 확인 (Player 태그가 설정되어 있어야 함)
        if (other.CompareTag("Player"))
        {
            if (UI_Manager.instance != null)
            {
                // 메시지 출력 없이 즉시 지도 열기
                //UI_Manager.instance.OpenWorldMap();
            }
        }
    }
}