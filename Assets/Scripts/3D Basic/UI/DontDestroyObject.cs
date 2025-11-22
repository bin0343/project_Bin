using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
    private void Awake()
    {
        // 씬이 바뀌어도 이 오브젝트는 파괴하지 마라
        DontDestroyOnLoad(gameObject);
    }
}