using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour    //카메라 바라보기
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam != null)
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up);
    }
}
