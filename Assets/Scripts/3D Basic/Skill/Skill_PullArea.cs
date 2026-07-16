using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PullDamageMode
{
    Continuous,
    SingleHit
}

[CreateAssetMenu(fileName = "NewPullAreaSkill", menuName = "Skill/Pull Area")]

public class Skill_PullArea : Skill_Base
{
    [Header("타겟팅")]
    [Tooltip("최대 사거리(적이 있으면 적한테 없으면 정면에)")]
    [SerializeField] private float castRange = 10f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 5.0f;    //바닥 찾기 위해 위에서 쏘는 높이

    [SerializeField] private GameObject pullFieldPrefab;

    [Header("스킬 작동 방식")]
    [SerializeField]
    private PullDamageMode damageMode =
    PullDamageMode.Continuous;
    [Tooltip("지속형 모드의 유지시간")]
    [SerializeField] private float fieldDuration = 2.5f;
    [Tooltip("단발형 모드에서 적을 끌어당기는 시간")]
    [SerializeField] private float singleFieldDuration = 0.4f;
    [Tooltip("지속형 모드의 반복 데미지 간격")]
    [SerializeField] private float damageInterval = 0.5f;

    [Tooltip("스킬 범위")]
    [SerializeField] private float pullRadius = 5f;
    [SerializeField] private float pullSpeed = 7f;
    [SerializeField] private float centerStopDistance = 0.5f;

    [SerializeField] private int damageAmount = 20;

    protected override void ApplyEffect(GameObject user, Vector3 targetPosition)
    {
        if (user == null) return;
        if (pullFieldPrefab == null) return;

        Player_Action player = user.GetComponent<Player_Action>();

        if (player == null)
        {
            player = user.GetComponentInParent<Player_Action>();
        }

        Animator animator = user.GetComponentInChildren<Animator>();

        Transform modelTransform = animator != null ? animator.transform : user.transform;

        Transform target = null;

        if (player != null)
        {
            target = player.FindNearestEnemyInRange(castRange);
        }

        Vector3 fieldPosition;

        if (target != null)
        {
            fieldPosition = target.position;
        }
        else
        {
            Vector3 forward = modelTransform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = user.transform.forward;
                forward.y = 0f;
            }

            forward.Normalize();

            fieldPosition = user.transform.position + forward * castRange;
        }

        fieldPosition = SnapToGround(fieldPosition);

        Vector3 lookDirection = fieldPosition - user.transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            modelTransform.rotation = Quaternion.LookRotation(lookDirection);
        }

        GameObject fieldObject = Instantiate(pullFieldPrefab, fieldPosition, Quaternion.identity);

        SkillPullField field = fieldObject.GetComponent<SkillPullField>();

        if (field == null)
        {
            Destroy(fieldObject);
            return;
        }

        LayerMask enemyLayer = player != null ? player.enemyLayer : LayerMask.GetMask("Enemy");

        float activeDuration = damageMode == PullDamageMode.Continuous ? fieldDuration : singleFieldDuration;

        field.Setup(player, damageAmount, activeDuration, pullRadius, pullSpeed, centerStopDistance, enemyLayer, damageMode, damageInterval);
    }

    private Vector3 SnapToGround(Vector3 position)
    {
        if (groundLayer == 0) return position;

        Vector3 rayOrigin = position + Vector3.up * groundCheckHeight;

        float rayDistance = groundCheckHeight * 2f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            return hit.point;
        }

        return position;
    }
}
