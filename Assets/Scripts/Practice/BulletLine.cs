using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLine : Bullet
{
    protected override void Move()
    {
        transform.Translate(Vector3.forward * Speed);
    }
}
