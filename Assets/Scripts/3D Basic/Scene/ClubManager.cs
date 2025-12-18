using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClubManager : MonoBehaviour
{
    [Header("--- 메인 화면 (동아리실) ---")]
    public Transform standingContainer; // 캐릭터들이 서 있을 부모 객체
    public GameObject standingPrefab;   // 캐릭터 스탠딩 이미지 프리팹 (Image 컴포넌트만 있는 것)

    [Header("--- 편성 팝업 ---")]
    public GameObject partyPopup;       // 팝업 패널 전체
    public Transform slotContent;       // ScrollView의 Content
    public GameObject slotPrefab;       // UI_PartySlot 프리팹
    public GameObject selectButton;

    [Header("--- 데이터 ---")]
    public List<NPC_Data> allNPCData;   // 게임에 존재하는 모든 NPC 데이터 (인스펙터에서 할당)

    // 현재 팝업에서 선택 중인 임시 파티 리스트
    private List<string> tempPartyList = new List<string>();

    void Start()
    {
        if (NPC_Manager.instance == null)
        {
            Debug.LogError("NPC Manager가 없습니다!");
            return;
        }

        // 2. [핵심] All NPC Data에 있는 애들을 매니저에 미리 등록시키는 과정
        if (allNPCData != null)
        {
            foreach (var data in allNPCData)
            {
                if (data != null)
                {
                    // 이 함수가 호출될 때 매니저 리스트(Inspector)에도 추가됩니다.
                    NPC_Manager.instance.GetNPCStatus(data.NPCID, data);
                }
            }
        }

        // 3. 화면 갱신
        RefreshMainStanding();
    }

    // =========================================================
    // 1. 메인 화면: 캐릭터 배치 (자동 정렬)
    // =========================================================
    public void RefreshMainStanding()
    {
        // 기존 이미지들 삭제
        foreach (Transform child in standingContainer)
        {
            Destroy(child.gameObject);
        }

        List<string> currentParty = NPC_Manager.instance.currentPartyIDs;

        // 파티원 수만큼 이미지 생성
        foreach (string id in currentParty)
        {
            NPC_Data npc = GetNPCDataByID(id);
            if (npc != null && npc.standingIllust != null)
            {
                GameObject go = Instantiate(standingPrefab, standingContainer);
                Image img = go.GetComponent<Image>();
                img.sprite = npc.standingIllust;
                img.preserveAspect = true;
            }
        }

        // ★ 중요: Layout Group 갱신을 위해 한 프레임 대기하거나 강제 업데이트 필요할 수 있음
        // 보통은 자동으로 됨.
    }

    // =========================================================
    // 2. 편성 팝업 관련
    // =========================================================
    public void OpenPartyPopup()
    {
        partyPopup.SetActive(true);
        selectButton.SetActive(false);

        // 현재 파티 상태를 임시 리스트에 복사
        tempPartyList = new List<string>(NPC_Manager.instance.currentPartyIDs);

        // 슬롯 생성
        RefreshPopupSlots();
    }

    void RefreshPopupSlots()
    {
        // 기존 슬롯 삭제
        foreach (Transform child in slotContent)
        {
            Destroy(child.gameObject);
        }

        // 영입된 NPC만 리스트에 표시
        foreach (NPC_Data npc in allNPCData)
        {
            if (NPC_Manager.instance.IsRecruited(npc.NPCID))
            {
                GameObject go = Instantiate(slotPrefab, slotContent);
                UI_PartySlot slot = go.GetComponent<UI_PartySlot>();

                // 현재 이 NPC가 임시 파티에 포함되어 있는지 확인
                bool isSelected = tempPartyList.Contains(npc.NPCID);
                slot.Setup(npc, isSelected, this);
            }
        }
    }

    // 슬롯이 클릭되었을 때 호출 (Toggle 로직)
    public bool OnSlotClicked(string npcID)
    {
        if (tempPartyList.Contains(npcID))
        {
            // 이미 있으면 제거 (선택 해제)
            tempPartyList.Remove(npcID);
            return true;
        }
        else
        {
            // 없으면 추가 (단, 3명 꽉 찼으면 불가)
            if (tempPartyList.Count >= 3)
            {
                Debug.Log("파티는 최대 3명까지만 가능합니다.");
                return false; // 변경 실패
            }

            tempPartyList.Add(npcID);
            return true; // 변경 성공
        }
    }

    // [확인] 버튼 클릭 시
    public void OnClickConfirm()
    {
        // NPC 매니저에 저장
        NPC_Manager.instance.SaveParty(tempPartyList);

        // 메인 화면 갱신
        RefreshMainStanding();

        // 팝업 닫기
        partyPopup.SetActive(false);
        selectButton.SetActive(true);
    }

    // [취소/X] 버튼 클릭 시
    public void OnClickClosePopup()
    {
        partyPopup.SetActive(false);
        selectButton.SetActive(true);
    }

    // 헬퍼 함수: ID로 Data 찾기
    NPC_Data GetNPCDataByID(string id)
    {
        return allNPCData.Find(x => x.NPCID == id);
    }
}