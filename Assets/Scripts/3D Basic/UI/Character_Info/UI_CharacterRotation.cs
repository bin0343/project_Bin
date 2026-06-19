using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CharacterRotation : MonoBehaviour, IDragHandler
{
    [Header("회전할 모델")]
    public Transform characterModel;

    [Header("캐릭터 애니메이션")]
    public Animator characterAnimator;

    [Header("회전 속도")]
    public float rotationSpeed = 0.3f;

    public void OnDrag(PointerEventData eventData)
    {
        if (characterModel == null || characterAnimator == null) return;

        AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);

        float rotX = eventData.delta.x * rotationSpeed;
        characterModel.Rotate(Vector3.up, -rotX, Space.World);
    }

    public void SetTarget(Transform newTarget, Animator newAnimator)
    {
        characterModel = newTarget;
        characterAnimator = newAnimator;
    }
}
