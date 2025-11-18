using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIconFollow : MonoBehaviour
{
    public Transform target;  // Player

    void LateUpdate()
    {
        transform.position = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z
        );

        // 아이콘의 방향을 플레이어에 맞추고 싶으면
        transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }
}
