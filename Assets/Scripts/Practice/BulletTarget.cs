using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTarget : Bullet
{
    protected Vector3 TargetPos = Vector3.zero;

    protected Vector3 Velocity = Vector3.zero;

    virtual public void InitBullet(int _Id, float _Speed, Vector3 _Target)
    {
        base.InitBullet(_Id, _Speed);

        TargetPos = _Target;
    }

    protected override void Move()
    {
        Velocity = TargetPos - transform.position;

        transform.position += Velocity * (Time.time * Speed);     //fixedDeltaTime = fixedUpdate에서, Deltatime = update에서
    }
}
