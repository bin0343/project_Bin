using UnityEngine;
using System.Collections.Generic;

public class DontDestroyObject : MonoBehaviour
{
    public static DontDestroyObject instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 씬이 바뀔 때 새로 생긴 복제본 UI_Canvas는 파괴
            Destroy(gameObject);
        }
    }
}