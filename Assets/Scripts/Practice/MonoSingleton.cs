using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T Inst = null;

    public static T Instance
    {
        get
        {
            if (Inst == null)
                Inst = (T)FindObjectOfType(typeof(T));

            if (FindObjectsOfType(typeof(T)).Length > 1)
                return Inst;

            if (Inst == null)
            {
                Inst = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();

                if (Inst == null)
                    Debug.Log("null");
            }

            return Inst;
        }
    }

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = (T)this;

            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
