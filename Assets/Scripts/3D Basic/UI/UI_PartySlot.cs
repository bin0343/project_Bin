using UnityEngine;
using UnityEngine.UI;

public class UI_PartySlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image imgFace;       // 얼굴 아이콘
    public Text txtName;        // 이름
    public Text txtLevel;       // 레벨
    public Text txtClass;       // 반 (검술반 등)
    public GameObject selectedOverlay; // 체크표시 + 어두운 배경

    [HideInInspector] public NPC_Data data;
    private bool isSelected = false;
    private ClubManager manager;

    public void Setup(NPC_Data npcData, bool selected, ClubManager mgr)
    {
        data = npcData;
        manager = mgr;
        isSelected = selected;

        // UI 갱신
        if (data != null)
        {
            // NPC_Data에 FaceImage가 따로 없다면 NPCImage 사용
            imgFace.sprite = data.NPCImage;
            txtName.text = data.NPCName;
            txtClass.text = data.classType.ToString();

            // 레벨은 매니저에서 가져옴
            int lvl = NPC_Manager.instance.GetNPCStatus(data.NPCID, data).level;
            txtLevel.text = $"Lv.{lvl}";
        }

        UpdateSelectionUI();
    }

    // 버튼 클릭 시 호출
    public void OnClickSlot()
    {
        // 매니저에게 나를 선택/해제 하겠다고 요청
        bool success = manager.OnSlotClicked(data.NPCID);

        if (success)
        {
            isSelected = !isSelected;
            UpdateSelectionUI();
        }
    }

    void UpdateSelectionUI()
    {
        if (selectedOverlay != null)
            selectedOverlay.SetActive(isSelected);
    }
}