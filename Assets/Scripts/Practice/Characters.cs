using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Particle;
using static UnityEngine.ParticleSystem;

public class Characters : MonoSingleton<Characters>
{
    public Vector3 originalscale;
    public Vector3 sitdown;
    public Vector3 baseposition;

    public GameObject Go;
    public Animator animator;

    public float atk;
    public float Def;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        //float hp = Monster.Instance.Atk - Def;
        originalscale = transform.localScale;
        baseposition = transform.localPosition;

        sitdown = new Vector3(originalscale.x, originalscale.y * 0.5f, originalscale.z);

        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()              //켜져있을 때 주기적으로 들어옴, 0.02초마다. 서버랑 동기화하거나 고정적으로 호출하는 부분, 이동 동기화. ex)pc마다 사양 차이날 경우
    {

    }

    // Update is called once per frame
    void Update()               //정해져 있지 않음.    나 혼자만 작동하는 곳에서 사용( ex) 캐릭터 로그인 시, 채팅, 솔로 플레이)
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            gameObject.SetActive(false);

            transform.gameObject.SetActive(false);          //트랜스폼에서 게임오브젝트에 접근
        }

        float movespeed = 0.01f;
        float jumpspeed = 1f;

        float movex = Input.GetAxisRaw("Horizontal");
        //float movey = Input.GetAxisRaw("Vertical");

        float scalespeed = 0.5f;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 scaleChange = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow)) scaleChange.y += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) scaleChange.y -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) scaleChange.x += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) scaleChange.x -= 1f;

            transform.localScale += scaleChange * scalespeed * Time.deltaTime;

            originalscale = transform.localScale;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))        //점프
            {
                if (transform.position != baseposition)
                {
                    transform.position += new Vector3(0f, jumpspeed, 0f);
                }
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))     //앉기
            {
                transform.localScale = sitdown;
            }
            else
            {
                transform.localScale = originalscale;
            }

            Vector3 moving = new Vector3(movex * movespeed, 0f, 0f);
            transform.position += moving;
        }

        //애니메이터 제어
        if (Input.GetKeyDown(KeyCode.X))
        {
            animator.SetTrigger("CharacterRun");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger("Attack");
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("Idle");
        }
    }

    public enum ANI
    {
        IDLE,
        RUN,
        END,
    }

    public ParticleSystem run;

    /*public void OnAnimationStart()
    {
        Go.SetActive(true);
    }

    public void OnAnimationEnd()
    {
        Go.SetActive(false);
    }

    public void OnAnimation(string _Anim)
    {
        if (_Anim == "start")
            Go.SetActive(true);
        else
            Go.SetActive(false);
    }*/
    public void PariticleAction(string Action)
    {
        if (!System.Enum.TryParse(Action, out ParticleAction action))
        {
            Debug.LogWarning($"[ParticleAction] 알 수 없는 명령: {Action}");
            return;
        }

        switch (action)
        {
            case ParticleAction.Idle:
                run.Stop();
                break;
            case ParticleAction.Run:
                run.Play();
                break;
            case ParticleAction.End:
                run.Stop();
                break;
        }
    }
}
