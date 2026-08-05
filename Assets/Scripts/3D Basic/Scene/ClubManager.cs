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
    public List<Character_Data> allNPCData;

    private List<string> tempPartyList = new List<string>();

    void Start()
    {
        if (Character_Manager.Instance == null)
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
                    Character_Manager.Instance.GetCharacterStatus(data.characterID, data);
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

        List<string> currentParty = Character_Manager.Instance.currentPartyIDs;

        foreach (string id in currentParty)
        {
            Character_Data npc = GetNPCDataByID(id);
            if (npc != null)
            {
                GameObject go = Instantiate(standingPrefab, standingContainer);
                Image img = go.GetComponent<Image>();
                img.preserveAspect = true;
            }
        }
    }

    public void OpenPartyPopup()
    {
        partyPopup.SetActive(true);
        selectButton.SetActive(false);

        tempPartyList = new List<string>(Character_Manager.Instance.currentPartyIDs);

        RefreshPopupSlots();
    }

    void RefreshPopupSlots()
    {
        foreach (Transform child in slotContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Character_Data npc in allNPCData)
        {
            if (Character_Manager.Instance.IsRecruited(npc.characterID))
            {
                GameObject go = Instantiate(slotPrefab, slotContent);
                UI_PartySlot slot = go.GetComponent<UI_PartySlot>();

                bool isSelected = tempPartyList.Contains(npc.characterID);
                //slot.Setup(npc, isSelected, this);
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
        List<Character_Data> selectedDataList = new List<Character_Data>();

        foreach (string id in tempPartyList)
        {
            Character_Data data = GetNPCDataByID(id);
            if (data != null)
            {
                selectedDataList.Add(data);
            }
        }
        Character_Manager.Instance.SaveParty(tempPartyList, selectedDataList);

        RefreshMainStanding();

        partyPopup.SetActive(false);
        selectButton.SetActive(true);
    }

    public void OnClickClosePopup()
    {
        partyPopup.SetActive(false);
        selectButton.SetActive(true);
    }

    Character_Data GetNPCDataByID(string id)
    {
        return allNPCData.Find(x => x.characterID == id);
    }
}