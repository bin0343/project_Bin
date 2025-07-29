using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    Dictionary<int, Bullet> DicBullets = new Dictionary<int, Bullet>();

    int BulletCount = 0;


    private void Awake()
    {
        Shared.BulletManager = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Bullet BulletFactory(BULLET _e, Transform _Tr)
    {
        Bullet bullet = null;

        switch (_e)
        {
            case BULLET.LINE:
                //bullet = new BulletLine();  //클래스 뒤에 monobehaviour가 없을때.
                {
                    UnityEngine.Object obj = Resources.Load("Prefabs/Practice/Bullet");  

                    GameObject go = obj as GameObject;      

                    go = Instantiate(go, _Tr.position, _Tr.rotation);
                }   //클래스 뒤에 monobehaviour가 있을때.
                break;
            case BULLET.TARGET:
                bullet = new BulletTarget();
                break;
        }

        return bullet;
    }

    public void CreateBulletPlayer(BULLET _e, Transform _Tr)
    {
        Bullet bullet = BulletFactory(_e, _Tr);

        if (bullet != null)
            return;

        bullet.InitBullet(BulletCount, 1f);

        DicBullets.Add(BulletCount, bullet);

        BulletCount++;
    }

    public void Destroy(Bullet _Bullet)
    {
        if (_Bullet == null)
            return;

        if (!DicBullets.ContainsKey(_Bullet.Id))
            return;

        DicBullets.Remove(_Bullet.Id);

        //오브젝트 풀링으로 변경하시오.
        Object.Destroy(_Bullet.gameObject);
    }
}
