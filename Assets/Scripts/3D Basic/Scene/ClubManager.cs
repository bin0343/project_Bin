using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClubManager : MonoBehaviour
{
    [Header("--- 메인 화면 (동아리실) ---")]
    public Transform standingContainer;
    public GameObject standingPrefab;

    [Header("--- 편성 팝업 ---")]
    public GameObject partyPopup; 
    public Transform slotContent;
    public GameObject slotPrefab;
    public GameObject selectButton;

    [Header("--- 데이터 ---")]
    public List<NPC_Data> allNPCData;

    private List<string> tempPartyList = new List<string>();

    void Start()
    {
        if (NPC_Manager.instance == null)
        {
            Debug.LogError("NPC Manager가 없습니다!");
            return;
        }

        if (allNPCData != null)
        {
            foreach (var data in allNPCData)
            {
                if (data != null)
                {
                    NPC_Manager.instance.GetNPCStatus(data.NPCID, data);
                }
            }
        }

        RefreshMainStanding();
    }

    public void RefreshMainStanding()
    {
        foreach (Transform child in standingContainer)
        {
            Destroy(child.gameObject);
        }

        List<string> currentParty = NPC_Manager.instance.currentPartyIDs;

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
    }

    public void OpenPartyPopup()
    {
        partyPopup.SetActive(true);
        selectButton.SetActive(false);

        tempPartyList = new List<string>(NPC_Manager.instance.currentPartyIDs);

        RefreshPopupSlots();
    }

    void RefreshPopupSlots()
    {
        foreach (Transform child in slotContent)
        {
            Destroy(child.gameObject);
        }

        foreach (NPC_Data npc in allNPCData)
        {
            if (NPC_Manager.instance.IsRecruited(npc.NPCID))
            {
                GameObject go = Instantiate(slotPrefab, slotContent);
                UI_PartySlot slot = go.GetComponent<UI_PartySlot>();

                bool isSelected = tempPartyList.Contains(npc.NPCID);
                slot.Setup(npc, isSelected, this);
            }
        }
    }

    public bool OnSlotClicked(string npcID)
    {
        if (tempPartyList.Contains(npcID))
        {
            tempPartyList.Remove(npcID);
            return true;
        }
        else
        {
            if (tempPartyList.Count >= 3)
            {
                Debug.Log("파티는 최대 3명까지만 가능합니다.");
                return false; 
            }

            tempPartyList.Add(npcID);
            return true;
        }
    }

    public void OnClickConfirm()
    {
        List<NPC_Data> selectedDataList = new List<NPC_Data>();

        foreach (string id in tempPartyList)
        {
            NPC_Data data = GetNPCDataByID(id);
            if (data != null)
            {
                selectedDataList.Add(data);
            }
        }
        NPC_Manager.instance.SaveParty(tempPartyList, selectedDataList);

        RefreshMainStanding();

        partyPopup.SetActive(false);
        selectButton.SetActive(true);
    }

    public void OnClickClosePopup()
    {
        partyPopup.SetActive(false);
        selectButton.SetActive(true);
    }

    NPC_Data GetNPCDataByID(string id)
    {
        return allNPCData.Find(x => x.NPCID == id);
    }
}