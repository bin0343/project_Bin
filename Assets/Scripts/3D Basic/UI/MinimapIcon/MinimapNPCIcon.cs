using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapNPCIcon : MonoBehaviour
{
    public Transform npc;

    void LateUpdate()
    {
        transform.position = new Vector3(
            npc.position.x,
            transform.position.y,
            npc.position.z
        );
    }
}
