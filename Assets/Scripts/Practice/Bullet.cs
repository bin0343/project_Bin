using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Id;

    protected float Speed;

    protected float CurTime;

    public void InitBullet(int _id, float _Speed)
    {
        Id = _id;
        Speed = _Speed;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Destroy();
    }

    protected virtual void Move()
    {

    }

    protected virtual void Destroy()
    {
        CurTime += Time.deltaTime;

        if (CurTime > 5f)
            Shared.BulletManager.Destroy(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CurTime += Time.deltaTime;

        if (CurTime > 5f)
            Shared.BulletManager.Destroy(this);
    }
}
