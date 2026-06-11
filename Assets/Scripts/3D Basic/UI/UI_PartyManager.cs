using System.Collections.Generic;
using UnityEngine;

public class UI_PartyManager : MonoBehaviour
{
    public UI_MiniPortrait[] partySlots = new UI_MiniPortrait[3];

    void Update()
    {
        UpdatePartyUI();
    }

    public void UpdatePartyUI()
    {
        if (partySlots == null || partySlots.Length == 0) return;

        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] != null)
            {
                partySlots[i].gameObject.SetActive(false);
            }
        }

        Player_Stat localPlayer = UI_Manager.instance != null ? UI_Manager.instance.playerStat : null;
        if (localPlayer != null && partySlots[0] != null)
        {
            Sprite pIcon = localPlayer.playerData != null ? localPlayer.playerData.characterIcon : null;
            partySlots[0].gameObject.SetActive(true); // 활성화
            partySlots[0].UpdatePortrait(pIcon, localPlayer.currentHP, localPlayer.maxHP);
        }

        if (NPC_Manager.instance != null && NPC_Manager.instance.currentPartyData != null)
        {
            List<NPC_Data> partyData = NPC_Manager.instance.currentPartyData;
            int currentSlotIndex = 1; // NPC는 1번 슬롯부터 들어감

            for (int i = 0; i < partyData.Count; i++)
            {
                if (currentSlotIndex >= partySlots.Length) break;

                NPC_Data npcData = partyData[i];

                if (npcData != null && partySlots[currentSlotIndex] != null)
                {
                    partySlots[currentSlotIndex].gameObject.SetActive(true); // 활성화

                    float currentHp = GetNpcCurrentHp(npcData.NPCID);
                    float maxHp = GetNpcMaxHp(npcData.NPCID);

                    partySlots[currentSlotIndex].UpdatePortrait(npcData.NPCPortrait, currentHp, maxHp);

                    currentSlotIndex++; // 다음 슬롯으로 넘어감
                }
            }
        }
    }

    private float GetNpcCurrentHp(string npcID)
    {
        NPC_Stat[] allNpcs = FindObjectsOfType<NPC_Stat>();
        foreach (var npc in allNpcs)
        {
            if (npc.npcData != null && npc.npcData.NPCID == npcID)
            {
                return npc.currentHP;
            }
        }
        return GetNpcMaxHp(npcID);
    }

    private float GetNpcMaxHp(string npcID)
    {
        NPCStatus status = NPC_Manager.instance.GetNPCStatus(npcID);
        if (status != null && status.currentStats != null)
        {
            return status.currentStats[(int)STAT.HP];
        }
        return 100f; // 기본값
    }
}