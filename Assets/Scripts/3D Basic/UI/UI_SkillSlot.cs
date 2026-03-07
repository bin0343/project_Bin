using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_SkillSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image SkillIcon;
    public Image CooldownMask;
    public Text CooldownText;
    public Text MpCostText;

    private SkillHolder assignedSkillHolder;
    public int slotIndex; // 각 슬롯이 자신의 인덱스를 알도록 변수 추가

    private Player_Action playerAction; // 데이터 교환을 위해 Player_Action 참조

    public bool IsEmpty => assignedSkillHolder == null;

    void Start()
    {
        playerAction = FindObjectOfType<Player_Action>(); // 씬에서 Player_Action을 찾아 연결
    }

    void Awake()
    {
        Clear();
    }

    public void Setup(SkillHolder skillHolder)
    {
        assignedSkillHolder = skillHolder;
        SkillIcon.sprite = skillHolder.SkillData.skillIcon;
        SkillIcon.enabled = true;
        //MpCostText.text = $"{skillHolder.SkillData.mpCost}";
        MpCostText.enabled = true;
        // 초기 쿨타임 UI 업데이트
        UpdateCooldownUI();
    }

    public void Clear()
    {
        assignedSkillHolder = null;
        SkillIcon.sprite = null;
        SkillIcon.enabled = false;
        MpCostText.text = "";
        MpCostText.enabled = false;
        CooldownMask.fillAmount = 0;
        CooldownText.enabled = false;
    }

    // REMOVED: StartCooldown(), GetSkill(), IsOnCooldown() 등 불필요한 메서드 제거

    void Update()
    {
        if (IsEmpty) return;
        UpdateCooldownUI();
    }

    private void UpdateCooldownUI()
    {
        float remaining = assignedSkillHolder.GetRemainingCooldown();
        float totalCooldown = assignedSkillHolder.SkillData.cooldownTime;

        if (remaining > 0)
        {
            CooldownMask.fillAmount = remaining / totalCooldown;
            CooldownText.enabled = true;
            CooldownText.text = remaining.ToString("F1");
        }
        else
        {
            CooldownMask.fillAmount = 0f;
            CooldownText.enabled = false;
        }
    }

    #region Drag and Drop Events
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return; // 빈 슬롯은 드래그 불가

        // DragSlot에 드래그 시작을 알림
        //DragSlot.StartDrag(SkillIcon, assignedSkillHolder, slotIndex);
        // 원래 아이콘을 잠시 투명하게 만듦
        SkillIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;
        // 마우스 위치로 고스트 아이콘 이동
        DragSlot.dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝났음을 알리고 원래 아이콘을 다시 보이게 함
        DragSlot.EndDrag();
        SkillIcon.color = new Color(1, 1, 1, 1);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 자기 자신에게 드롭하는 경우는 무시
        if (DragSlot.originalIndex == slotIndex) return;

        // 드롭 위치(현재 슬롯)와 드래그 시작 위치의 데이터를 교환하도록 요청
        //playerAction.SwapSkill(DragSlot.originalIndex, slotIndex);
    }
    #endregion
}
