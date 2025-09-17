using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public partial class MainCamera : MonoBehaviour
{
    bool CameraShake = false;

    Transform ShakeTr;
    Vector3 baseLocalPos;

    public class cShakeInfo
    {
        public float StartDelay;
        public bool UseTotalTime;
        public float TotalTime;
        public Vector3 Dest;
        public Vector3 Shake;
        public Vector3 Dir;

        public float RemainDist;
        public float RemainCountDis;

        public bool UseCount;
        public int Count;

        public float Veclocity;

        public bool UseDamping;
        public float Damping;
        public float DampingTime;
    }

    cShakeInfo ShakeInfo = new cShakeInfo();

    Vector3 OrgPos;

    float FovX = 0.2f;
    float FovY = 0.2f;

    private void Awake()
    {
        Shared.MainCamera = this;

        OrgPos = transform.position;

        InitShake();
    }

    protected void InitShake()
    {
        ShakeTr = transform.parent;
        baseLocalPos = ShakeTr.localPosition;
        CameraShake = false;
    }

    protected void ResetShakeTr()
    {
        ShakeTr.localPosition = baseLocalPos;
        CameraShake = false;

        CameraLimit();
    }

    void CameraLimit(bool OrgPosY = false)
    {
        Vector3 camera = OrgPos;

        if (camera.x - FovX < -1f)      //왼쪽
            camera.x = -1 + FovX;
        else if (camera.x - FovX > 1f)      //오른쪽
            camera.x = 1f - FovX;

        if (OrgPosY)
            camera.y = OrgPos.y;
    }

    public void Shake( int CameraID = 0)
    {
        /*if (false == IsFollowme)
            return;*/

        ShakeInfo.StartDelay = 0f;
        ShakeInfo.TotalTime = 3f;
        ShakeInfo.UseTotalTime = true;

        ShakeInfo.Shake = new Vector3(0.2f, 0.2f, 0f);

        ShakeInfo.Dest = ShakeInfo.Shake;
        ShakeInfo.Dir = ShakeInfo.Shake;
        ShakeInfo.Dir.Normalize();

        ShakeInfo.RemainDist = ShakeInfo.Shake.magnitude;
        ShakeInfo.RemainCountDis = float.MaxValue;

        ShakeInfo.Veclocity = 8;

        ShakeInfo.Damping = 0.5f;
        ShakeInfo.UseDamping = true;

        ShakeInfo.DampingTime = ShakeInfo.RemainDist / ShakeInfo.Veclocity;

        ShakeInfo.Count = 4;
        ShakeInfo.UseCount = true;

        StopCoroutine("ShakeCoroutine");

        ResetShakeTr();

        StartCoroutine("ShakeCoroutine");
    }

    IEnumerator ShakeCoroutine()
    {
        CameraShake = true;

        float dt, dist;

        if (ShakeInfo.StartDelay > 0f)
            yield return new WaitForSeconds(ShakeInfo.StartDelay);

        while (true)
        {
            dt = Time.fixedDeltaTime;
            dist = dt * ShakeInfo.Veclocity;

            if ((ShakeInfo.RemainDist -= dist) > 0)
            {
                ShakeTr.localPosition += ShakeInfo.Dir * dist;
            }
            else
            {
                if (ShakeInfo.UseDamping)
                {
                    float distdamping = Mathf.Max(ShakeInfo.Damping * ShakeInfo.DampingTime, ShakeInfo.Damping * dt);

                    if (ShakeInfo.Shake.magnitude > distdamping)
                        ShakeInfo.Shake -= ShakeInfo.Dir * distdamping;
                    else
                    {
                        ShakeInfo.UseCount = true;
                        ShakeInfo.Count = 1;
                    }
                }

                ShakeTr.localPosition += ShakeInfo.Dest - ShakeInfo.Dir * (-ShakeInfo.RemainDist);

                ShakeInfo.Shake = -ShakeInfo.Shake;
                ShakeInfo.Dest = -ShakeInfo.Shake;
                ShakeInfo.Dir = -ShakeInfo.Dir;

                float len = ShakeInfo.Shake.magnitude;

                ShakeInfo.RemainCountDis = len + ShakeInfo.RemainDist;
                ShakeInfo.RemainDist += len * 2f;

                ShakeInfo.DampingTime = ShakeInfo.RemainDist / ShakeInfo.Veclocity;

                if (ShakeInfo.RemainDist < dist)
                    break;
            }

            // 카메라 위치가 부드럽게 원래 위치로 돌아오도록 Lerp 함수를 사용합니다.
            ShakeTr.localPosition = Vector3.Lerp(ShakeTr.localPosition, baseLocalPos, Time.fixedDeltaTime * ShakeInfo.Veclocity);

            if (ShakeInfo.UseTotalTime && (ShakeInfo.TotalTime -= dt) < 0)
                break;

            yield return new WaitForFixedUpdate();
        }

        // 코루틴 종료 후 카메라 위치를 원래 위치로 확실히 되돌립니다.
        ShakeTr.localPosition = baseLocalPos;
        CameraShake = false;

        yield break;
    }
}
