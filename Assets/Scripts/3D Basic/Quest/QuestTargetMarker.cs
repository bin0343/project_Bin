using UnityEngine;
using System.Collections.Generic;

public class QuestTargetMarker : MonoBehaviour
{
    [Header("퀘스트 목표 ID")]
    [Tooltip("Quest 오브젝트에 적은 targetID와 똑같이 적어주세요 (예: OBJ_LostCat)")]
    public string targetID;

    private static Dictionary<string, Transform> targetRegistry = new Dictionary<string, Transform>();

    private void OnEnable()
    {
        // 오브젝트가 켜질 때 명부에 자기 자신을 등록
        if (!string.IsNullOrEmpty(targetID))
        {
            targetRegistry[targetID] = this.transform;
        }
    }

    private void OnDisable()
    {
        // 오브젝트가 꺼지거나 파괴될 때 명부에서 삭제
        if (!string.IsNullOrEmpty(targetID) && targetRegistry.ContainsKey(targetID))
        {
            targetRegistry.Remove(targetID);
        }
    }

    // 외부(NPC)에서 ID만 주면 위치를 즉시 찾아주는 함수
    public static Transform GetTarget(string id)
    {
        if (targetRegistry.ContainsKey(id))
        {
            return targetRegistry[id];
        }
        return null;
    }
}