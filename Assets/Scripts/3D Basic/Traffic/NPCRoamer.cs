using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCRoamer : MonoBehaviour
{
    private NavMeshAgent agent;

    // [나중에 3D 캐릭터 모델 추가 시 주석 해제]
    // private Animator anim; 

    [Header("직진 성향 설정")]
    public float walkDistance = 30f; // 한 번에 목표로 잡을 전방 거리

    [Header("상호작용 상태 (심플 Interactable 연동)")]
    public bool isInteracting = false;

    // 횡단보도를 건너는 중인지 체크
    private bool isCrossing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // [나중에 3D 캐릭터 모델 추가 시 주석 해제]
        // anim = GetComponentInChildren<Animator>(); 

        WalkForward();
    }

    void Update()
    {
        // 상호작용(대화 등) 중일 때는 이동 정지
        if (isInteracting)
        {
            agent.isStopped = true;

            // [나중에 3D 캐릭터 모델 추가 시 주석 해제 (대기 애니메이션 전환)]
            // if (anim != null) anim.SetFloat("Speed", 0f); 

            return;
        }

        agent.isStopped = false;

        // 목적지에 거의 다다랐거나, 막혀서 경로를 잃었을 때
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                if (isCrossing)
                {
                    // 횡단보도를 다 건넜다면, 뒤돌지 않고 그 방향 그대로 다시 직진
                    isCrossing = false;
                    WalkForward();
                }
                else
                {
                    // 일반적인 인도/광장 끝에 다다랐다면, 방향을 틀어서 다시 걷기
                    TurnAroundAndWalk();
                }
            }
        }

        // [나중에 3D 캐릭터 모델 추가 시 주석 해제 (걷기 애니메이션 전환)]
        // if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    // 앞쪽으로 목표 지점을 던져서 길을 찾게 하는 함수
    public void WalkForward()
    {
        Vector3 forwardPoint = transform.position + (transform.forward * walkDistance);
        NavMeshHit hit;

        if (NavMesh.SamplePosition(forwardPoint, out hit, 10f, agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            TurnAroundAndWalk();
        }
    }

    // 방향을 틀고 걷는 함수
    void TurnAroundAndWalk()
    {
        float randomAngle = Random.Range(120f, 240f);
        transform.Rotate(0, randomAngle, 0);

        WalkForward();
    }

    // 횡단보도 진입로 트리거 (StopCube 센서)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CrosswalkEntry") && !isCrossing)
        {
            if (Random.value > 0.5f) // 50% 확률로 횡단 결심
            {
                CrosswalkPoint crossPoint = other.GetComponent<CrosswalkPoint>();
                if (crossPoint != null && crossPoint.oppositePoint != null)
                {
                    // 목적지를 건너편 횡단보도 지점으로 강제 변경
                    agent.SetDestination(crossPoint.oppositePoint.position);
                    isCrossing = true;
                }
            }
        }
    }
}