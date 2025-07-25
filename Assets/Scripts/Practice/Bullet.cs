using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Bullet : MonoBehaviour
{
    protected float Speed;

    public void InitBullet(float _Speed)
    {
        Speed = _Speed;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    protected virtual void Move()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
