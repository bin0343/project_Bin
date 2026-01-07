using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
    private void Awake()
    {
        var objs = FindObjectsOfType<DontDestroyObject>();

        foreach (var obj in objs)
        {
            // 나(this)와 다른 오브젝트인데, 이름이 같다면? (중복 발견)
            if (obj != this && obj.gameObject.name == gameObject.name)
            {
                // 3. 늦게 생성된 나(새로운 씬의 Canvas)를 파괴합니다.
                // (기존에 살아있는 Canvas를 유지하기 위함)
                Destroy(gameObject);
                return;
            }
        }

        // 4. 중복이 없다면 나를 보존합니다.
        DontDestroyOnLoad(gameObject);
    }
}