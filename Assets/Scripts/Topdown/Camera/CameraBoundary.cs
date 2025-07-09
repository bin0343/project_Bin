using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundary : MonoBehaviour
{
    public Vector2 minPosition;
    public Vector2 maxPosition;

    void Start()
    {
        CameraFollow camera = FindObjectOfType<CameraFollow>();
        if (camera != null)
        {
            camera.minPosition = minPosition;
            camera.maxPosition = maxPosition;

            if (camera.target == null)
            {
                camera.target = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }
    }
}
